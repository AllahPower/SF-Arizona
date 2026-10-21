using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.RakNet.Hooks;

internal unsafe class OutgoingPacketHook : NativeHook<nint, bool, OutgoingPacketHook.SendBitStreamNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    [return: MarshalAs(UnmanagedType.U1)]
    internal unsafe delegate bool SendBitStreamNative(
        nint thisPtr,
        nint bitStream,
        int priority,
        int reliability,
        byte orderingChannel);

    private readonly NetworkFilters _filters;
    private readonly NetworkDispatcher _dispatcher;

    internal OutgoingPacketHook(NetworkFilters filters, NetworkDispatcher dispatcher)
    {
        _filters = filters;
        _dispatcher = dispatcher;
        nint targetAddress = ModuleResolver.GetProcAddress("samp.dll", SampOffsets.RakClientRuntime.SendBitStream);
        InstallHook(targetAddress, new SendBitStreamNative(HookProc));
    }

    private bool HookProc(
        nint thisPtr,
        nint bitStream,
        int priority,
        int reliability,
        byte orderingChannel)
    {
        try
        {

            if (bitStream != 0)
            {
                if (RakNetHookPayloadReader.TryReadPacketBitStream((CRakNetBitStream*)bitStream, out int packetId, out int bitsUsed, out ReadOnlySpan<byte> payload))
                {
                    byte* data = ((CRakNetBitStream*)bitStream)->Data;

                    if (_filters.OutgoingPacket.HasFilters &&
                        _filters.OutgoingPacket.ShouldCancel(packetId, data, bitsUsed))
                    {
                        return true;
                    }

                    if (_dispatcher.OutgoingPacketHandlers.HasSubscribers(packetId))
                    {
                        _dispatcher.EnqueueOutgoingPacket(packetId, payload.ToArray(), bitsUsed);
                    }
                }
            }
        }
        catch (Exception ex) { HookRuntime.ReportSafely(ex); }

        try { return OriginalFunction(thisPtr, bitStream, priority, reliability, orderingChannel); }
        catch (Exception ex) { HookRuntime.ReportSafely(ex); return false; }
    }

    protected override bool InvokeOriginalFunction(nint args)
    {
        throw new NotSupportedException();
    }

}
