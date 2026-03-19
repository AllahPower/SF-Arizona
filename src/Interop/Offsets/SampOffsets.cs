namespace SFSharp;

public static class SampOffsets
{
    public static class CNetGame
    {
        public const int Instance = 0x26E8DC;
        public const int Constructor = 0xB5F0;
        public const int RakClient = 0x2C;
        public const int GetPlayerPool = 0x1160;
        public const int UpdatePlayers = 0x8BA0;
    }

    public static class CInput
    {
        public const int Instance = 0x26E8CC;
        public const int Send = 0x69190;
    }

    public static class CDialog
    {
        public const int Instance = 0x26E898;
        public const int Show = 0x6F8C0;
        public const int Hide = 0x6F110;
        public const int Close = 0x6FF40;
    }

    public static class CLocalPlayer
    {
        public const int Chat = 0x5820;
    }

    public static class CPlayerPool
    {
        public const int GetLocalPlayer = 0x1A30;
        public const int GetLocalPlayerName = 0xA170;
        public const int GetName = 0x16F00;
        public const int GetScore = 0x6E0E0;
    }

    public static class CScoreboard
    {
        public const int UpdateScoresPingsIps = 0x10090;
    }

    public static class SampFuncs
    {
        public const int CDialogClose = 0x91265;
    }

    public static class CChat
    {
        public const int Instance = 0x26E8C8;
        public const int AddEntry = 0x67460;
        public const int AddChatMessage = 0x678A0;
        public const int AddMessage = 0x679F0;
    }

    public static class RakClientVTable
    {
        public const int EntryCount = 55;
        public const int Destructor = 0;
        public const int Connect = 1;
        public const int Disconnect = 2;
        public const int InitializeSecurity = 3;
        public const int SetPassword = 4;
        public const int HasPassword = 5;
        public const int Send_Data = 6;
        public const int Send_BitStream = 7;
        public const int Receive = 8;
        public const int DeallocatePacket = 9;
        public const int PingServer = 10;
        public const int GetAveragePing = 12;
        public const int GetLastPing = 13;
        public const int GetLowestPing = 14;
        public const int IsConnected = 18;
        public const int RegisterAsRemoteProcedureCall = 22;
        public const int RegisterClassMemberRpc = 23;
        public const int UnregisterAsRemoteProcedureCall = 24;
        public const int RpcData = 25;
        public const int RpcBitStream = 26;
        public const int GetStaticServerData = 34;
        public const int GetPlayerIndex = 54;
    }

    public static class RpcRuntime
    {
        public const int HandleRpcPacket = 0x3A6A0;
        // RakClient::RPC (BitStream overload, vtable[25])
        // bool __thiscall RPC(this, int* uniqueID, BitStream* bitStream, priority, reliability, orderingChannel, shiftTimestamp)
        public const int SendRpcBitStream = 0x33EE0;
    }

    public static class RakNetBitStream
    {
        public const int NumberOfBitsUsed = 4;
        public const int Data = 12;
    }

    public static class RakNetPacket
    {
        // Packet struct returned by RakClient::Receive
        public const int Length = 8;
        public const int BitSize = 12;
        public const int Data = 16;
    }

    public static class RpcRoutes
    {
        public static class Chat
        {
            public const int AddChatMessage = CChat.AddChatMessage;
            public const int AddMessage = CChat.AddMessage;
        }

        public static class Dialog
        {
            public const int Show = CDialog.Show;
            public const int Close = CDialog.Close;
        }

        public static class Game
        {
            public const int DisplayGameText = 0xA05D0;
            public const int DeleteRacingCheckpoint = 0xA0630;
            public const int SetCheckpoint = 0xA16B0;
            public const int SetRacingCheckpoint = 0xA19D0;
        }
    }
}

