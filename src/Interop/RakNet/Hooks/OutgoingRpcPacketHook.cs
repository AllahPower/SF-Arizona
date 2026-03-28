using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp;

internal unsafe class OutgoingRpcPacketHook : NativeHook<nint, bool, OutgoingRpcPacketHook.OutgoingRpcPacketNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate bool OutgoingRpcPacketNative(
        nint thisPtr,
        int* uniqueID,
        nint bitStream,
        int priority,
        int reliability,
        byte orderingChannel,
        [MarshalAs(UnmanagedType.U1)] bool shiftTimestamp);

    private const int BitStreamNumberOfBitsUsed = 4;
    private const int BitStreamData = 12;

    private static OutgoingRpcPacketHook? _instance;

    public OutgoingRpcPacketHook()
    {
        _instance = this;
        InstallHook(
            ModuleResolver.GetProcAddress("samp.dll", SampOffsets.RpcRuntime.SendRpcBitStream),
            new OutgoingRpcPacketNative(HookProc));
    }

    private static unsafe bool HookProc(
        nint thisPtr,
        int* uniqueID,
        nint bitStream,
        int priority,
        int reliability,
        byte orderingChannel,
        bool shiftTimestamp)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        if (uniqueID != null && bitStream != 0)
        {
            int rpcId = *uniqueID;

            if (rpcId >= 0 && SFBootstrap.OutgoingRpcHandlers.HasSubscribers(rpcId))
            {
                int dataBitLength = *(int*)(bitStream + BitStreamNumberOfBitsUsed);
                byte* data = *(byte**)(bitStream + BitStreamData);
                int dataByteLength = (dataBitLength + 7) / 8;

                byte[] packet = new byte[dataByteLength];
                if (data != null && dataByteLength > 0)
                {
                    fixed (byte* dst = packet)
                    {
                        Buffer.MemoryCopy(data, dst, dataByteLength, dataByteLength);
                    }
                }

                SFBootstrap.EnqueueOutgoingRpc(rpcId, packet, dataBitLength);
            }
        }

        return _instance.OriginalFunction(thisPtr, uniqueID, bitStream, priority, reliability, orderingChannel, shiftTimestamp);
    }

    protected override bool InvokeOriginalFunction(nint args)
    {
        throw new NotSupportedException();
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
