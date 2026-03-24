using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp;

// Hook on AZVoice.asi PluginInterface::OnReceive to capture incoming packet 252
// before the plugin consumes it from the RakNet receive queue.
// Without this hook, packet 252 is invisible to IncomingPacketHook because
// AZVoice's RakNet plugin processes and swallows it.
internal unsafe class IncomingAZVoicePacketHook : NativeHook<nint, int, IncomingAZVoicePacketHook.PluginOnReceiveNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate int PluginOnReceiveNative(nint thisPtr, nint peerPtr, nint packetPtr);

    // ARZ::OnReceivePacket offset from AZVoice.asi image base (0x10000000)
    private const uint OnReceivePacketOffset = 0x41FB6;
    private const byte PacketIdAZVoice = 252; // 0xFC

    private static IncomingAZVoicePacketHook? _instance;

    public IncomingAZVoicePacketHook()
    {
        _instance = this;
        nint azvoiceBase = (nint)Win32.GetModuleHandle("AZVoice.asi");
        nint targetAddress = azvoiceBase + (nint)OnReceivePacketOffset;
        InstallHook(targetAddress, new PluginOnReceiveNative(HookProc));
    }

    private static unsafe int HookProc(nint thisPtr, nint peerPtr, nint packetPtr)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        if (packetPtr != 0)
        {
            byte* data = *(byte**)(packetPtr + SampOffsets.RakNetPacket.Data);
            int length = *(int*)(packetPtr + SampOffsets.RakNetPacket.Length);

            if (TryGetNormalizedPacket(data, length, out byte packetId, out byte* normalizedData, out int normalizedLength))
            {
                if (packetId == PacketIdAZVoice && SFBootstrap.IncomingPacketHandlers.HasSubscribers(packetId))
                {
                    int bitSize = normalizedLength * 8;
                    byte[] packet = new byte[normalizedLength];
                    fixed (byte* dst = packet)
                    {
                        Buffer.MemoryCopy(normalizedData, dst, normalizedLength, normalizedLength);
                    }

                    SFBootstrap.EnqueueIncomingPacket(packetId, packet, bitSize);
                }
            }
        }

        return _instance.OriginalFunction(thisPtr, peerPtr, packetPtr);
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
}
