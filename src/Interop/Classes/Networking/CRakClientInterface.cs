using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using unsafe DeallocatePacketDelegate = delegate* unmanaged[Thiscall]<nint, nint, void>;
using unsafe PushBackPacketDelegate = delegate* unmanaged[Thiscall]<nint, nint, bool, void>;
using unsafe ReceiveDelegate = delegate* unmanaged[Thiscall]<nint, nint>;
using unsafe RpcBitStreamDelegate = delegate* unmanaged[Thiscall]<nint, int*, nint, int, int, byte, bool, bool>;
using unsafe RpcDataDelegate = delegate* unmanaged[Thiscall]<nint, int*, byte*, uint, int, int, byte, bool, bool>;
using unsafe SendBitStreamDelegate = delegate* unmanaged[Thiscall]<nint, nint, int, int, byte, bool>;
using unsafe SendDataDelegate = delegate* unmanaged[Thiscall]<nint, byte*, int, int, int, byte, bool>;

namespace SFSharp.Runtime.Interop.Classes.Networking;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct CRakClientInterface
{
    private readonly CRakClientInterfaceVTable* __vftable;

    public CRakClientInterfaceVTable* VTable => __vftable;
    public bool IsAvailable => __vftable != null;

    public nint GetVirtualFunctionAddress(int slot)
    {
        return __vftable == null ? 0 : ((nint*)__vftable)[slot];
    }

    public bool Send(CRakNetBitStream* bitStream, RakNetPacketPriority priority = RakNetPacketPriority.High, RakNetPacketReliability reliability = RakNetPacketReliability.Reliable, byte orderingChannel = 0)
    {
        if (__vftable == null || bitStream == null)
        {
            return false;
        }

        SendBitStreamDelegate send = (SendBitStreamDelegate)__vftable->SendBitStream;
        return send == null ? false : send((nint)Unsafe.AsPointer(ref this), (nint)bitStream, (int)priority, (int)reliability, orderingChannel);
    }

    public bool Send(byte* data, int lengthBytes, RakNetPacketPriority priority = RakNetPacketPriority.High, RakNetPacketReliability reliability = RakNetPacketReliability.Reliable, byte orderingChannel = 0)
    {
        if (__vftable == null || data == null || lengthBytes <= 0)
        {
            return false;
        }

        SendDataDelegate send = (SendDataDelegate)__vftable->SendData;
        return send == null ? false : send((nint)Unsafe.AsPointer(ref this), data, lengthBytes, (int)priority, (int)reliability, orderingChannel);
    }

    public bool Send(ReadOnlySpan<byte> data, RakNetPacketPriority priority = RakNetPacketPriority.High, RakNetPacketReliability reliability = RakNetPacketReliability.Reliable, byte orderingChannel = 0)
    {
        if (data.IsEmpty)
        {
            return false;
        }

        fixed (byte* dataPtr = data)
        {
            return Send(dataPtr, data.Length, priority, reliability, orderingChannel);
        }
    }

    public CRakNetPacket* Receive()
    {
        if (__vftable == null)
        {
            return null;
        }

        ReceiveDelegate receive = (ReceiveDelegate)__vftable->Receive;
        return receive == null ? null : (CRakNetPacket*)receive((nint)Unsafe.AsPointer(ref this));
    }

    public void DeallocatePacket(CRakNetPacket* packet)
    {
        if (__vftable == null || packet == null)
        {
            return;
        }

        DeallocatePacketDelegate deallocatePacket = (DeallocatePacketDelegate)__vftable->DeallocatePacket;
        deallocatePacket((nint)Unsafe.AsPointer(ref this), (nint)packet);
    }

    public bool TryReceive(out CRakNetPacket* packet)
    {
        packet = Receive();
        return packet != null;
    }

    public bool Rpc(int rpcId, CRakNetBitStream* bitStream, RakNetPacketPriority priority = RakNetPacketPriority.High, RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered, byte orderingChannel = 0, bool shiftTimestamp = false)
    {
        if (__vftable == null || bitStream == null)
        {
            return false;
        }

        int id = rpcId;
        RpcBitStreamDelegate rpc = (RpcBitStreamDelegate)__vftable->RpcBitStream;
        return rpc == null ? false : rpc((nint)Unsafe.AsPointer(ref this), &id, (nint)bitStream, (int)priority, (int)reliability, orderingChannel, shiftTimestamp);
    }

    public void PushBackPacket(CRakNetPacket* packet, bool pushAtHead = false)
    {
        if (__vftable == null || packet == null)
        {
            return;
        }

        PushBackPacketDelegate pushBack = (PushBackPacketDelegate)__vftable->PushBackPacket;
        if (pushBack != null)
        {
            pushBack((nint)Unsafe.AsPointer(ref this), (nint)packet, pushAtHead);
        }
    }

    public bool Rpc(int rpcId, ReadOnlySpan<byte> payload, int payloadBitLength, RakNetPacketPriority priority = RakNetPacketPriority.High, RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered, byte orderingChannel = 0, bool shiftTimestamp = false)
    {
        if (__vftable == null || payloadBitLength < 0)
        {
            return false;
        }

        fixed (byte* payloadPtr = payload)
        {
            int id = rpcId;
            RpcDataDelegate rpc = (RpcDataDelegate)__vftable->RpcData;
            return rpc == null ? false : rpc((nint)Unsafe.AsPointer(ref this), &id, payloadPtr, (uint)payloadBitLength, (int)priority, (int)reliability, orderingChannel, shiftTimestamp);
        }
    }

}
