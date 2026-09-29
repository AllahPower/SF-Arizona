using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Networking;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CRakClientInterfaceVTable
{
    public nint Destructor;
    public nint Connect;
    public nint Disconnect;
    public nint InitializeSecurity;
    public nint SetPassword;
    public nint HasPassword;
    public nint SendData;
    public nint SendBitStream;
    public nint Receive;
    public nint DeallocatePacket;
    public nint PingServer;
    public nint PingServer_0;
    public nint GetAveragePing;
    public nint GetLastPing;
    public nint GetLowestPing;
    public nint GetPlayerPing;
    public nint StartOccasionalPing;
    public nint StopOccasionalPing;
    public nint IsConnected;
    public nint GetSynchronizedRandomInteger;
    public nint GenerateCompressionLayer;
    public nint DeleteCompressionLayer;
    public nint RegisterAsRemoteProcedureCall;
    public nint RegisterClassMemberRPC;
    public nint UnregisterAsRemoteProcedureCall;
    public nint RpcData;
    public nint RpcBitStream;
    public nint RpcBitStreamToNetworkId;
    public nint SetTrackFrequencyTable;
    public nint GetSendFrequencyTable;
    public nint GetCompressionRatio;
    public nint GetDecompressionRatio;
    public nint AttachPlugin;
    public nint DetachPlugin;
    public nint GetStaticServerData;
    public nint SetStaticServerData;
    public nint GetStaticClientData;
    public nint SetStaticClientData;
    public nint SendStaticClientDataToServer;
    public nint GetServerID;
    public nint GetPlayerID;
    public nint GetInternalID;
    public nint PlayerIDToDottedIP;
    public nint PushBackPacket;
    public nint SetRouterInterface;
    public nint RemoveRouterInterface;
    public nint SetTimeoutTime;
    public nint SetMTUSize;
    public nint GetMTUSize;
    public nint AllowConnectionResponseIPMigration;
    public nint AdvertiseSystem;
    public nint GetStatistics;
    public nint ApplyNetworkSimulator;
    public nint IsNetworkSimulatorActive;
    public nint GetPlayerIndex;
}
