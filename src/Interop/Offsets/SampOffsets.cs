namespace SFSharp;

public static class SampOffsets
{
    public static class CNetGame
    {
        public const int Instance = 0x26E8DC;
        public const int Constructor = 0xB5F0;
        public const int State = 0x3CD;
        public const int Pools = 0x3DE;
        public const int RakClient = 0x2C;
        public const int GetPlayerPool = 0x1160;
        public const int GetVehiclePool = 0x1170;
        public const int GetObjectPool = 0x2DF0;
        public const int GetActorPool = 0x2E00;
        public const int GetState = 0x2E10;
        public const int GetPickupPool = 0x8170;
        public const int GetMenuPool = 0x8180;
        public const int SetState = 0x8190;
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
        public const int GetPed = 0x2D50;
        public const int GetSpecialAction = 0x3340;
        public const int SetColor = 0x3D50;
        public const int GetColorAsRgba = 0x3D80;
        public const int GetColorAsArgb = 0x3DA0;
        public const int RequestSpawn = 0x3ED0;
        public const int Chat = 0x5820;
    }

    public static class CPlayerPool
    {
        public const int MaxPlayers = 1004;
        public const int RemotePlayerInfoArray = 0x004;
        public const int ConnectionStateArray = 0xFB4;
        public const int LocalPlayerPing = 0x2F14;
        public const int LocalPlayerScore = 0x2F18;
        public const int LocalPlayerId = 0x2F1C;
        public const int LocalPlayerName = 0x2F22;
        public const int LocalPlayerPointer = 0x2F3A;
        public const int PlayerInfoPing = 0x04;
        public const int PlayerInfoName = 0x0C;
        public const int PlayerInfoScore = 0x24;
        public const int PlayerInfoNpcFlag = 0x28;
        public const int IsConnected = 0x10B0;
        public const int GetPlayer = 0x10F0;
        public const int GetLocalPlayer = 0x1A30;
        public const int GetCount = 0x13670;
        public const int GetLocalPlayerName = 0xA170;
        public const int GetName = 0x16F00;
        public const int GetScore = 0x6E0E0;
        public const int GetPing = 0x6E110;
        public const int GetLocalPlayerPing = 0x6E150;
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
        public const int Send_BitStream = 6;
        public const int Send_Data = 7;
        public const int Receive = 8;
    }

    public static class RpcRuntime
    {
        public const int HandleRpcPacket = 0x3A6A0;
        public const int SendRpcBitStream = 0x33EE0;
    }

    public static class SampStringCompressor
    {
        public const int Instance = 0x534F0;
        public const int DecodeString = 0x53B90;
    }

    public static class RakNetBitStream
    {
        public const int NumberOfBitsUsed = 4;
        public const int Data = 12;
    }

    public static class RakNetPacket
    {
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
