using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SFSharp;

// MinHook detour on RakClient::Receive (vtable[8])
// Packet* __thiscall Receive(this)
internal unsafe class IncomingPacketHook : NativeHook<nint, nint, IncomingPacketHook.ReceiveNative>
{
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    internal unsafe delegate nint ReceiveNative(nint thisPtr);

    private static IncomingPacketHook? _instance;

    public IncomingPacketHook()
    {
        _instance = this;
        nint targetAddress = ModuleResolver.ResolveVTableFunction(
            "samp.dll",
            SampOffsets.CNetGame.Instance,
            SampOffsets.CNetGame.RakClient,
            SampOffsets.RakClientVTable.Receive);
        InstallHook(targetAddress, new ReceiveNative(HookProc));
    }

    private static unsafe nint HookProc(nint thisPtr)
    {
        if (_instance is null)
        {
            throw new UnreachableException();
        }

        nint packetPtr = _instance.OriginalFunction(thisPtr);

        if (packetPtr != 0)
        {
            int bitSize = *(int*)(packetPtr + SampOffsets.RakNetPacket.BitSize);
            byte* data = *(byte**)(packetPtr + SampOffsets.RakNetPacket.Data);

            if (data != null && bitSize >= 8)
            {
                int packetId = data[0];

                if (SFBootstrap.IncomingPacketHandlers.HasSubscribers(packetId))
                {
                    int dataByteLength = (bitSize + 7) / 8;
                    byte[] packet = new byte[dataByteLength];
                    fixed (byte* dst = packet)
                    {
                        Buffer.MemoryCopy(data, dst, dataByteLength, dataByteLength);
                    }

                    SFBootstrap.EnqueueIncomingPacket(packetId, packet, bitSize);
                }
            }
        }

        return packetPtr;
    }

    protected override nint InvokeOriginalFunction(nint args)
    {
        throw new NotSupportedException();
    }

    public override void Dispose()
    {
        base.Dispose();
        _instance = null;
    }
}
