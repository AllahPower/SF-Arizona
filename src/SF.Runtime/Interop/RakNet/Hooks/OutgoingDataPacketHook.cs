using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.RakNet.Hooks;

internal sealed unsafe class OutgoingDataPacketHook : NativeHook<nint, bool, OutgoingDataPacketHook.SendDataNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal delegate bool SendDataNative(nint thisPtr, byte* data, int length, int priority, int reliability, byte orderingChannel);

    private readonly NetworkFilters _filters;
    private readonly NetworkDispatcher _dispatcher;

    internal OutgoingDataPacketHook(NetworkFilters filters, NetworkDispatcher dispatcher)
    {
        _filters = filters;
        _dispatcher = dispatcher;
        InstallHook(ModuleResolver.GetProcAddress("samp.dll", SampOffsets.RakClientRuntime.SendData), new SendDataNative(HookProc));
    }

    private bool HookProc(nint thisPtr, byte* data, int length, int priority, int reliability, byte orderingChannel)
    {
        try
        {
            if (data != null && length > 0 && length <= int.MaxValue / 8)
            {
                int packetId = data[0];
                int bitLength = length * 8;
                if (_filters.OutgoingPacket.HasFilters && _filters.OutgoingPacket.ShouldCancel(packetId, data, bitLength))
                {
                    return true;
                }

                if (_dispatcher.OutgoingPacketHandlers.HasSubscribers(packetId))
                {
                    _dispatcher.EnqueueOutgoingPacket(packetId, new ReadOnlySpan<byte>(data, length).ToArray(), bitLength);
                }
            }
        }
        catch (Exception ex) { HookRuntime.ReportSafely(ex); }

        try { return OriginalFunction(thisPtr, data, length, priority, reliability, orderingChannel); }
        catch (Exception ex) { HookRuntime.ReportSafely(ex); return false; }
    }

    protected override bool InvokeOriginalFunction(nint args) => throw new NotSupportedException();

}