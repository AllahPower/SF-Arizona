using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Network.RakNet.Hooks;

// Hook on AZVoice.asi control-RPC dispatcher for packet 252 sub-RPC payloads.
// Unlike ARZ::OnReceivePacket, this path handles control RPC 3..23, not voice frames.
//
// The target is resolved by a wildcarded byte pattern rather than a fixed module
// offset: AZVoice.asi is relinked between builds, so a hard offset silently drifts
// (the old 0x53A3D landed in the middle of the Opus decode path, which is what made
// installing the trampoline crash). Matching the function prologue keeps resolution
// stable across relinks, exactly like IncomingAZVoicePacketHook.
internal unsafe class IncomingAZVoiceRpcHook : NativeHook<nint, int, IncomingAZVoiceRpcHook.RpcDispatcherNative>
{
    private const string ModuleName = "AZVoice.asi";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int RpcDispatcherNative(nint descriptorPtr);

    // AzVoice_OnIncoming252 / control-RPC dispatcher.
    // Restored from IDA at 0x10055665:
    // - MSVC SEH prologue (push ebp/edi/esi/ebx, sub esp, install EH frame)
    // - once-init guard on byte_1033AC48 that builds the sub-id -> handler table (3..23)
    // - reads the RPCParameters descriptor: input ptr (+0), bit length (+4),
    //   sender PlayerID (+8/+12) checked against UNASSIGNED, then dispatches by sub-id.
    // Stack displacements, absolute data addresses and rel32 call/jmp targets are
    // wildcarded so minor module relinks do not break resolution; opcodes, the memset
    // count (37 dwords) and the first control sub-id (3) anchor it uniquely.
    private static readonly byte?[] RpcDispatcherPattern =
    [
        0x55,                                                       // push ebp
        0x89, 0xE5,                                                 // mov ebp, esp
        0x57,                                                       // push edi
        0x56,                                                       // push esi
        0x8D, 0x85, null, null, null, null,                         // lea eax, [ebp+scope]
        0x53,                                                       // push ebx
        0x81, 0xEC, null, null, null, null,                         // sub esp, imm32
        0x89, 0xAD, null, null, null, null,                         // mov [ebp+var], ebp
        0x89, 0x04, 0x24,                                           // mov [esp], eax
        0xC7, 0x85, null, null, null, null, null, null, null, null, // mov [ebp+var], offset EH_handler
        0xC7, 0x85, null, null, null, null, null, null, null, null, // mov [ebp+var], offset EH_data
        0xC7, 0x85, null, null, null, null, null, null, null, null, // mov [ebp+var], offset scope
        0x89, 0xA5, null, null, null, null,                         // mov [ebp+var], esp
        0xE8, null, null, null, null,                               // call SEH setup
        0xA0, null, null, null, null,                               // mov al, ds:once-init guard
        0x84, 0xC0,                                                 // test al, al
        0x0F, 0x85, null, null, null, null,                         // jnz (already initialised)
        0xC7, 0x04, 0x24, null, null, null, null,                   // mov [esp], offset guard
        0xE8, null, null, null, null,                               // call lock/acquire
        0x85, 0xC0,                                                 // test eax, eax
        0x0F, 0x84, null, null, null, null,                         // jz (skip table build)
        0x8D, 0xBD, null, null, null, null,                         // lea edi, [ebp+table]
        0xB9, 0x25, 0x00, 0x00, 0x00,                               // mov ecx, 37 (dwords)
        0x31, 0xC0,                                                 // xor eax, eax
        0xF3, 0xAB,                                                 // rep stosd
        0xC6, 0x85, null, null, null, null, 0x03,                   // mov byte ptr [ebp+table], 3
    ];

    private static nint _dispatcherAddress;
    private static bool _resolved;

    private static IncomingAZVoiceRpcHook? _instance;

    public static bool IsAvailable => ResolveTargetAddress() && _dispatcherAddress != 0;

    public IncomingAZVoiceRpcHook()
    {
        if (!ResolveTargetAddress() || _dispatcherAddress == 0)
        {
            throw new InvalidOperationException("AZVoice incoming RPC hook target could not be resolved.");
        }

        _instance = this;
        InstallHook(_dispatcherAddress, new RpcDispatcherNative(HookProc));
    }

    private static int HookProc(nint descriptorPtr)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        TryEnqueueControlPacket(descriptorPtr);
        return _instance.OriginalFunction(descriptorPtr);
    }

    protected override int InvokeOriginalFunction(nint args)
    {
        throw new NotSupportedException();
    }

    private static unsafe void TryEnqueueControlPacket(nint descriptorPtr)
    {
        if (descriptorPtr == 0)
        {
            return;
        }

        byte* payloadPtr = *(byte**)descriptorPtr;
        int payloadBits = *(int*)(descriptorPtr + 4);
        int discriminatorA = *(int*)(descriptorPtr + 8);
        ushort discriminatorB = *(ushort*)(descriptorPtr + 12);

        if (payloadPtr == null || payloadBits <= 7 || (discriminatorA == -1 && discriminatorB == 0xFFFF))
        {
            return;
        }

        int payloadBytes = (payloadBits + 7) / 8;
        byte[] packet = new byte[payloadBytes + 1];
        packet[0] = 252;

        fixed (byte* dst = &packet[1])
        {
            Buffer.MemoryCopy(payloadPtr, dst, payloadBytes, payloadBytes);
        }

        if (payloadBytes >= 1 && SFBootstrap.IncomingAZVoiceControlHandlers.HasSubscribers(packet[1]))
        {
            SFBootstrap.EnqueueIncomingAZVoiceControl(packet[1], packet, payloadBits + 8);
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }

    private static bool ResolveTargetAddress()
    {
        if (_resolved)
        {
            return true;
        }

        if (!ModuleResolver.IsModuleLoaded(ModuleName))
        {
            return false;
        }

        _dispatcherAddress = ModuleResolver.FindPattern(ModuleName, RpcDispatcherPattern);
        if (_dispatcherAddress == 0)
        {
            SFLog.Warn("AZVoice incoming RPC hook pattern not found.");
        }
        else
        {
            SFLog.Debug($"Resolved AZVoice incoming RPC hook target at 0x{_dispatcherAddress:X8}.");
        }

        _resolved = true;
        return true;
    }
}
