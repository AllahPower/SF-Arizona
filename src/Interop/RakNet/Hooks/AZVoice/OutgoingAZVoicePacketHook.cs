using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Network.RakNet.Hooks;

// Debug hook on AZVoice.asi voice-frame sender for outgoing packet 252.
// Target: AzVoice_BuildAndSendVoiceFrame — it builds the RakNet BitStream
// (0xFC id, u16 packet number, u8 stream id, aligned Opus payload) and hands it
// to RakClient::Send. Hooking here observes AZVoice's outbound voice traffic at
// the source, before it is folded into samp's generic Send path.
//
// Resolved by a wildcarded prologue pattern (ModuleResolver.FindPattern), like the
// incoming AZVoice hooks: AZVoice.asi is relinked between builds, so a fixed module
// offset would silently drift onto the wrong function.
internal unsafe class OutgoingAZVoicePacketHook : NativeHook<nint, int, OutgoingAZVoicePacketHook.SendVoiceFrameNative>
{
    private const string ModuleName = "AZVoice.asi";

    // int __thiscall AzVoice_BuildAndSendVoiceFrame(micStream* this, const void* opus, uint opusLength)
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate int SendVoiceFrameNative(nint thisPtr, nint opusData, uint opusLength);

    // AzVoice_BuildAndSendVoiceFrame — restored from IDA at 0x1003EAF4.
    // Prologue anchors: WriteBits(8-bit) argument setup and the literal 0xFC packet
    // id store; stack displacements, absolute addresses and rel32 targets wildcarded.
    private static readonly byte?[] SendVoiceFramePattern =
    [
        0x55,                                                       // push ebp
        0x89, 0xE5,                                                 // mov ebp, esp
        0x57,                                                       // push edi
        0x56,                                                       // push esi
        0x8D, 0xBD, null, null, null, null,                         // lea edi, [ebp+scratch]
        0x89, 0xCE,                                                 // mov esi, ecx (this)
        0x53,                                                       // push ebx
        0x8D, 0x9D, null, null, null, null,                         // lea ebx, [ebp+bitstream]
        0x81, 0xEC, null, null, null, null,                         // sub esp, imm32
        0xC7, 0x85, null, null, null, null, null, null, null, null, // mov [ebp+var], offset EH_handler
        0x89, 0x3C, 0x24,                                           // mov [esp], edi
        0xC7, 0x85, null, null, null, null, null, null, null, null, // mov [ebp+var], offset EH_data
        0xE8, null, null, null, null,                               // call SEH setup
        0x8D, 0x8D, null, null, null, null,                         // lea ecx, [ebp+hdr]
        0xC7, 0x85, null, null, null, null, 0x00, 0x00, 0x00, 0x00, // mov [ebp+var], 0
        0xE8, null, null, null, null,                               // call BitStream_Ctor
        0x89, 0x1C, 0x24,                                           // mov [esp], ebx
        0x8D, 0x8D, null, null, null, null,                         // lea ecx, [ebp+hdr]
        0xC7, 0x44, 0x24, 0x08, 0x01, 0x00, 0x00, 0x00,             // mov [esp+8], 1 (write-aligned flag)
        0xC7, 0x44, 0x24, 0x04, 0x08, 0x00, 0x00, 0x00,             // mov [esp+4], 8 (bit count)
        0xC6, 0x85, null, null, null, null, 0xFC,                   // mov byte ptr [ebp+hdr], 0FCh (packet 252)
        0xE8, null, null, null, null,                               // call BitStream_WriteBits
        0x66, 0x8B, 0x86, 0x82, 0x00, 0x00, 0x00,                   // mov ax, [esi+82h] (packet counter)
    ];

    // Offsets into the micStream object, from the IDA prologue above.
    private const int StreamIdOffset = 33;      // *((byte*)this + 33)
    private const int PacketCounterOffset = 0x82; // *((ushort*)this + 65)
    private const int PayloadPreviewBytes = 48;

    private static nint _senderAddress;
    private static bool _resolved;

    private static OutgoingAZVoicePacketHook? _instance;

    public static bool IsAvailable => ResolveTargetAddress() && _senderAddress != 0;

    public OutgoingAZVoicePacketHook()
    {
        if (!ResolveTargetAddress() || _senderAddress == 0)
        {
            throw new InvalidOperationException("AZVoice outgoing packet hook target could not be resolved.");
        }

        _instance = this;
        InstallHook(_senderAddress, new SendVoiceFrameNative(HookProc));
    }

    private static int HookProc(nint thisPtr, nint opusData, uint opusLength)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        if (thisPtr != 0)
        {
            byte streamId = *(byte*)(thisPtr + StreamIdOffset);
            ushort packetNumber = (ushort)(*(ushort*)(thisPtr + PacketCounterOffset) + 1);
            string preview = HexPreview(opusData, opusLength);
            SFLog.Debug(
                $"AZVoice OUT packet 252: stream={streamId} packet#={packetNumber} opus={opusLength}B [{preview}]");
        }

        return _instance.OriginalFunction(thisPtr, opusData, opusLength);
    }

    private static string HexPreview(nint data, uint length)
    {
        if (data == 0 || length == 0)
        {
            return string.Empty;
        }

        int take = (int)Math.Min(length, PayloadPreviewBytes);
        var span = new ReadOnlySpan<byte>((void*)data, take);
        string hex = Convert.ToHexString(span);
        return length > PayloadPreviewBytes ? hex + "…" : hex;
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

        _senderAddress = ModuleResolver.FindPattern(ModuleName, SendVoiceFramePattern);
        if (_senderAddress == 0)
        {
            SFLog.Warn("AZVoice outgoing packet hook pattern not found.");
        }
        else
        {
            SFLog.Debug($"Resolved AZVoice outgoing packet hook target at 0x{_senderAddress:X8}.");
        }

        _resolved = true;
        return true;
    }
}
