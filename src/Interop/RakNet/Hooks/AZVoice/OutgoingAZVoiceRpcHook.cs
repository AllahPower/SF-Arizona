using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Network.RakNet.Hooks;

// Debug hook on AZVoice.asi control-RPC sender for outgoing packet 252.
// Target: AzVoice_SendRpcToServer — it writes the sub-op byte and an optional body
// BitStream, then dispatches through RakClient::RPC (unique id 252). Every AZVoice
// control message the client emits funnels through here, so hooking it observes the
// outbound control channel at the source.
//
// Resolved by a wildcarded prologue pattern (ModuleResolver.FindPattern), like the
// other AZVoice hooks: a fixed module offset drifts when AZVoice.asi is relinked.
internal unsafe class OutgoingAZVoiceRpcHook : NativeHook<nint, int, OutgoingAZVoiceRpcHook.SendRpcNative>
{
    private const string ModuleName = "AZVoice.asi";

    // int __thiscall AzVoice_SendRpcToServer(this, int subOp, BitStream* body)
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate int SendRpcNative(nint thisPtr, int subOp, nint body);

    // AzVoice_SendRpcToServer — restored from IDA at 0x100420CC.
    // Prologue anchors: the SEH frame stores, the -1 BitStream sentinel init and the
    // sub-op byte load; stack displacements, absolute addresses and rel32 targets
    // wildcarded so relinks do not break resolution.
    private static readonly byte?[] SendRpcPattern =
    [
        0x55,                                                       // push ebp
        0x89, 0xE5,                                                 // mov ebp, esp
        0x81, 0xEC, null, null, null, null,                         // sub esp, imm32
        0x8B, 0x45, 0x08,                                           // mov eax, [ebp+subOp]
        0x89, 0x8D, null, null, null, null,                         // mov [ebp+var], ecx (this)
        0xC7, 0x85, null, null, null, null, null, null, null, null, // mov [ebp+var], offset EH_handler
        0x89, 0x85, null, null, null, null,                         // mov [ebp+var], eax
        0x8D, 0x85, null, null, null, null,                         // lea eax, [ebp+scope]
        0x89, 0x04, 0x24,                                           // mov [esp], eax
        0xC7, 0x85, null, null, null, null, null, null, null, null, // mov [ebp+var], offset EH_data
        0x89, 0xAD, null, null, null, null,                         // mov [ebp+var], ebp
        0xC7, 0x85, null, null, null, null, null, null, null, null, // mov [ebp+var], offset scope
        0x89, 0xA5, null, null, null, null,                         // mov [ebp+var], esp
        0xE8, null, null, null, null,                               // call SEH setup
        0x8D, 0x8D, null, null, null, null,                         // lea ecx, [ebp+bitstream]
        0xC7, 0x85, null, null, null, null, 0xFF, 0xFF, 0xFF, 0xFF, // mov [ebp+var], -1 (BitStream sentinel)
        0xE8, null, null, null, null,                               // call BitStream_Ctor
        0x8A, 0x85, null, null, null, null,                         // mov al, byte ptr [ebp+subOp]
    ];

    private const int PayloadPreviewBytes = 48;

    // AZVoice's own RakNet BitStream layout: numberOfBitsUsed at +0, data pointer at +12.
    private const int BitStreamNumberOfBitsUsed = 0;
    private const int BitStreamData = 12;

    private static nint _senderAddress;
    private static bool _resolved;

    private static OutgoingAZVoiceRpcHook? _instance;

    public static bool IsAvailable => ResolveTargetAddress() && _senderAddress != 0;

    public OutgoingAZVoiceRpcHook()
    {
        if (!ResolveTargetAddress() || _senderAddress == 0)
        {
            throw new InvalidOperationException("AZVoice outgoing RPC hook target could not be resolved.");
        }

        _instance = this;
        InstallHook(_senderAddress, new SendRpcNative(HookProc));
    }

    private static int HookProc(nint thisPtr, int subOp, nint body)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        SFLog.Debug($"AZVoice OUT RPC 252: subOp={subOp & 0xFF} body=[{DescribeBody(body)}]");

        return _instance.OriginalFunction(thisPtr, subOp, body);
    }

    private static string DescribeBody(nint body)
    {
        if (body == 0)
        {
            return "none";
        }

        int bitsUsed = *(int*)(body + BitStreamNumberOfBitsUsed);
        byte* data = *(byte**)(body + BitStreamData);
        if (data == null || bitsUsed <= 0 || bitsUsed > (8 << 20))
        {
            return "empty";
        }

        int byteLength = (bitsUsed + 7) / 8;
        int take = Math.Min(byteLength, PayloadPreviewBytes);
        string hex = Convert.ToHexString(new ReadOnlySpan<byte>(data, take));
        string suffix = byteLength > PayloadPreviewBytes ? "…" : string.Empty;
        return $"{byteLength}B {hex}{suffix}";
    }

    protected override int InvokeOriginalFunction(nint args)
    {
        throw new NotSupportedException();
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

        _senderAddress = ModuleResolver.FindPattern(ModuleName, SendRpcPattern);
        if (_senderAddress == 0)
        {
            SFLog.Warn("AZVoice outgoing RPC hook pattern not found.");
        }
        else
        {
            SFLog.Debug($"Resolved AZVoice outgoing RPC hook target at 0x{_senderAddress:X8}.");
        }

        _resolved = true;
        return true;
    }
}
