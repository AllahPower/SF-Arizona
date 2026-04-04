using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp;

// Hook on AZVoice.asi PluginInterface::OnReceive to capture incoming packet 252
// before the plugin consumes it from the RakNet receive queue.
// Without this hook, packet 252 is invisible to IncomingPacketHook because
// AZVoice's RakNet plugin processes and swallows it.
internal unsafe class IncomingAZVoicePacketHook : NativeHook<nint, int, IncomingAZVoicePacketHook.PluginOnReceiveNative>
{
    private const string ModuleName = "AZVoice.asi";
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate int PluginOnReceiveNative(nint thisPtr, nint peerPtr, nint packetPtr);

    // ARZ_OnReceivePacket252 / PluginInterface::OnReceive.
    // Restored from IDA at 0x10041FB6:
    // - function starts by building EH frame state
    // - calls ARZ_GetPacketTypeAndOffset
    // - checks for packet id 0xFC
    // - for accepted voice-frame payloads calls ARZ_ParseVoiceDataPacket252
    // Absolute addresses and rel32 calls are wildcarded so minor module relinks do not break resolution.
    private static readonly byte?[] OnReceivePacketPattern =
    [
        0x55, 0x89, 0xE5, 0x53, 0x8D, 0x85, null, null, null, null, 0x81, 0xEC, 0x74, 0x01, 0x00, 0x00,
        0x89, 0x8D, null, null, null, null, 0x89, 0x04, 0x24,
        0xC7, 0x85, null, null, null, null, null, null, null, null,
        0xC7, 0x85, null, null, null, null, null, null, null, null,
        0x89, 0xAD, null, null, null, null,
        0xC7, 0x85, null, null, null, null, null, null, null, null,
        0x89, 0xA5, null, null, null, null,
        0xE8, null, null, null, null,
        0x8D, 0x85, null, null, null, null,
        0xC7, 0x85, null, null, null, null, 0xFF, 0xFF, 0xFF, 0xFF,
        0x89, 0x44, 0x24, 0x04, 0x8B, 0x45, 0x0C, 0x89, 0x04, 0x24,
        0xE8, null, null, null, null,
        0xC7, 0x85, null, null, null, null, 0x01, 0x00, 0x00, 0x00,
        0x3C, 0xFC
    ];

    private const byte PacketIdAZVoice = 252; // 0xFC
    private static nint _onReceivePacketAddress;
    private static bool _resolved;

    private static IncomingAZVoicePacketHook? _instance;

    public static bool IsAvailable => ResolveTargetAddress() && _onReceivePacketAddress != 0;

    public IncomingAZVoicePacketHook()
    {
        if (!ResolveTargetAddress() || _onReceivePacketAddress == 0)
        {
            throw new InvalidOperationException("AZVoice incoming packet hook target could not be resolved.");
        }

        _instance = this;
        InstallHook(_onReceivePacketAddress, new PluginOnReceiveNative(HookProc));
    }

    private static unsafe int HookProc(nint thisPtr, nint peerPtr, nint packetPtr)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        byte[]? normalizedPacket = null;
        byte normalizedPacketId = 0;
        int normalizedBitSize = 0;

        if (packetPtr != 0)
        {
            byte* data = *(byte**)(packetPtr + SampOffsets.RakNetPacket.Data);
            int length = *(int*)(packetPtr + SampOffsets.RakNetPacket.Length);

            if (TryGetNormalizedPacket(data, length, out byte packetId, out byte* normalizedData, out int normalizedLength))
            {
                if (packetId == PacketIdAZVoice && SFBootstrap.IncomingAZVoiceDataHandlers.HasSubscribers())
                {
                    normalizedBitSize = normalizedLength * 8;
                    normalizedPacketId = packetId;
                    normalizedPacket = new byte[normalizedLength];
                    fixed (byte* dst = normalizedPacket)
                    {
                        Buffer.MemoryCopy(normalizedData, dst, normalizedLength, normalizedLength);
                    }
                }
            }
        }

        int result = _instance.OriginalFunction(thisPtr, peerPtr, packetPtr);

        // ARZ::OnReceivePacket returns 0 only when packet 252 was accepted as voice data
        // and consumed by the plugin. Control RPCs continue through a different dispatcher.
        if (result == 0 && normalizedPacket is not null)
        {
            SFBootstrap.EnqueueIncomingAZVoiceData(normalizedPacket, normalizedBitSize);
        }

        return result;
    }

    protected override int InvokeOriginalFunction(nint args)
    {
        throw new NotSupportedException();
    }

    private static bool TryGetNormalizedPacket(byte* data, int length, out byte packetId, out byte* normalizedData, out int normalizedLength)
    {
        packetId = 0;
        normalizedData = null;
        normalizedLength = 0;

        if (data == null || length <= 0)
        {
            return false;
        }

        int packetOffset = data[0] == 40 ? 5 : 0; // ARZ::GetPacketTypeAndOffset
        if (length <= packetOffset)
        {
            return false;
        }

        packetId = data[packetOffset];
        normalizedData = data + packetOffset;
        normalizedLength = length - packetOffset;
        return true;
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

        _onReceivePacketAddress = ModuleResolver.FindPattern(ModuleName, OnReceivePacketPattern);
        if (_onReceivePacketAddress == 0)
        {
            SFLog.Warn("AZVoice incoming packet hook pattern not found.");
        }
        else
        {
            SFLog.Info($"Resolved AZVoice incoming packet hook target at 0x{_onReceivePacketAddress:X8}.");
        }

        _resolved = true;
        return true;
    }
}
