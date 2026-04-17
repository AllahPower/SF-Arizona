using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

// Hook on AZVoice.asi control dispatcher for packet 252 sub-RPC payloads.
// Unlike ARZ::OnReceivePacket, this path handles control RPC 3..23, not voice frames.
internal unsafe class IncomingAZVoiceRpcHook : NativeHook<nint, int, IncomingAZVoiceRpcHook.RpcDispatcherNative>
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate int RpcDispatcherNative(nint descriptorPtr);

    private const uint RpcDispatcherOffset = 0x53A3D;
    private const byte PacketIdAZVoice = 252;

    private static IncomingAZVoiceRpcHook? _instance;

    public IncomingAZVoiceRpcHook()
    {
        _instance = this;
        nint azvoiceBase = (nint)Win32.GetModuleHandle("AZVoice.asi");
        nint targetAddress = azvoiceBase + (nint)RpcDispatcherOffset;
        InstallHook(targetAddress, new RpcDispatcherNative(HookProc));
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
        packet[0] = PacketIdAZVoice;

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
}
