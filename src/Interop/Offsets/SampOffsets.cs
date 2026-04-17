namespace SFSharp.Runtime.Interop;

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
        public const int LanMode = 0x2E20;
        public const int GetPickupPool = 0x8170;
        public const int GetMenuPool = 0x8180;
        public const int SetState = 0x8190;
        public const int InitializePools = 0x81D0;
        public const int InitialNotification = 0x83F0;
        public const int GetCounter = 0x8570;
        public const int InitializeGameLogic = 0x8580;
        public const int Connect = 0x85D0;
        public const int SpawnScreen = 0x8640;
        public const int GetRakClient = 0x1A40;
        public const int UpdatePlayers = 0x8BA0;
        public const int ResetPlayerPool = 0x8C70;
        public const int ResetVehiclePool = 0x8D10;
        public const int ResetTextDrawPool = 0x8DB0;
        public const int ResetObjectPool = 0x8E50;
        public const int ResetGangZonePool = 0x8EF0;
        public const int ResetPickupPool = 0x8F90;
        public const int ResetMenuPool = 0x8FF0;
        public const int ResetLabelPool = 0x9080;
        public const int ResetActorPool = 0x9120;
        public const int ResetMarkers = 0x9F50;
        public const int ResetPools = 0xA190;
        public const int ShutdownForRestart = 0xA1E0;
    }

    public static class CVehiclePool
    {
        public const int MaxVehicles = 2000;
        public const int Get = 0x1110;
        public const int DoesExist = 0x1140;
        public const int UpdateCount = 0x1E260;
        public const int Delete = 0x1E330;
        public const int ChangeInterior = 0x1E3B0;
        public const int SetParams = 0x1E3E0;
        public const int GetNearest = 0x1E4B0;
        public const int ProcessWaitingList = 0x1EBC0;
        public const int Process = 0x1EC80;
    }

    public static class CObjectPool
    {
        public const int MaxObjects = 1000;
        public const int LargestId = 0x0000;
        public const int OccupancyArray = 0x0004;
        public const int ObjectArray = 0x0FA4;
        public const int Get = 0x2DC0;
        public const int GetCount = 0x124E0;
        public const int Delete = 0x12500;
        public const int Create = 0x12580;
        public const int GetId = 0x126C0;
        public const int Process = 0x12700;
        public const int ConstructMaterials = 0x127C0;
        public const int ShutdownMaterials = 0x12800;
        public const int Draw = 0x12840;
        public const int DrawLast = 0x12880;
    }

    public static class CObject
    {
        public const int Constructor = 0xA8880;
        public const int GetDistance = 0xA7730;
        public const int Stop = 0xA77A0;
        public const int SetRotation = 0xA7810;
        public const int SetAttachedToVehicle = 0xA7880;
        public const int SetAttachedToObject = 0xA7910;
        public const int AttachToVehicle = 0xA79B0;
        public const int AttachToObject = 0xA7A30;
        public const int Rotate = 0xA7B30;
        public const int AttachedToMovingEntity = 0xA7C30;
        public const int SetMaterial = 0xA7CA0;
        public const int SetMaterialText = 0xA7E20;
        public const int GetMaterialSize = 0xA83F0;
        public const int ShutdownMaterialText = 0xA8640;
        public const int Render = 0xA86D0;
        public const int Process = 0xA8DC0;
        public const int ConstructMaterialText = 0xA9650;
        public const int Draw = 0xA9700;
    }

    public static class CActorPool
    {
        public const int MaxActors = 1000;
        public const int LargestId = 0x0000;
        public const int ObjectArray = 0x0004;
        public const int OccupancyArray = 0x0FA4;
        public const int Get = 0x1600;
        public const int DoesExist = 0x1630;
        public const int Delete = 0x16E0;
        public const int Find = 0x18A0;
        public const int Create = 0x18F0;
    }

    public static class CActor
    {
        public const int Constructor = 0x9BBA0;
        public const int Destroy = 0x9BCF0;
        public const int PerformAnimation = 0x9BD50;
        public const int SetRotation = 0x9BE60;
        public const int GetHealth = 0x9BEA0;
        public const int SetHealth = 0x9BEC0;
        public const int SetInvulnerable = 0x9BFF0;
    }

    public static class CPickupPool
    {
        public const int MaxPickups = 4096;
        public const int Count = 0x0000;
        public const int Handles = 0x0004;
        public const int ServerIds = 0x4004;
        public const int Timers = 0x8004;
        public const int Weapons = 0xC004;
        public const int Pickups = 0xF004;
        public const int Constructor = 0x8130;
        public const int Destructor = 0x130C0;
        public const int CreateWeapon = 0x12E30;
        public const int Create = 0x12F20;
        public const int Delete = 0x12FD0;
        public const int DeleteWeapon = 0x13030;
        public const int GetIndex = 0x13090;
        public const int SendNotification = 0x130F0;
        public const int Process = 0x131D0;
    }

    public static class CMenuPool
    {
        public const int MaxMenus = 128;
        public const int MenuArray = 0x0000;
        public const int OccupancyArray = 0x0200;
        public const int CurrentMenu = 0x0400;
        public const int Cancelled = 0x0402;
        public const int Constructor = 0x7AE0;
        public const int Destructor = 0x7E50;
        public const int Create = 0x7B30;
        public const int Delete = 0x7C00;
        public const int Show = 0x7C50;
        public const int Hide = 0x7CB0;
        public const int GetTextPointer = 0x7CF0;
        public const int Process = 0x7E90;
    }

    public static class CMenu
    {
        public const int Constructor = 0xA6CE0;
        public const int AddItem = 0xA6D80;
        public const int SetColumnTitle = 0xA6DB0;
        public const int Hide = 0xA6DE0;
        public const int GetItem = 0xA6E00;
        public const int GetTitle = 0xA6E20;
        public const int GetString = 0xA6E50;
        public const int GetActiveRow = 0xA6E80;
        public const int Show = 0xA6EB0;
    }

    public static class CGangZonePool
    {
        public const int MaxGangZones = 1024;
        public const int ObjectArray = 0x0000;
        public const int OccupancyArray = 0x1000;
        public const int Constructor = 0x2100;
        public const int Destructor = 0x2130;
        public const int Create = 0x2160;
        public const int StartFlashing = 0x21E0;
        public const int StopFlashing = 0x2200;
        public const int Delete = 0x2220;
        public const int Draw = 0x2250;
    }

    public static class CLabelPool
    {
        public const int MaxLabels = 2048;
        public const int ObjectArray = 0x0000;
        public const int OccupancyArray = 0xE800;
        public const int Constructor = 0x1180;
        public const int Destructor = 0x15D0;
        public const int Create = 0x11C0;
        public const int Delete = 0x12D0;
        public const int Draw = 0x1340;
    }

    public static class CTextDrawPool
    {
        public const int MaxTextDraws = 2048;
        public const int MaxLocalTextDraws = 256;
        public const int TotalTextDraws = MaxTextDraws + MaxLocalTextDraws;
        public const int OccupancyArray = 0x0000;
        public const int ObjectArray = 0x2400;
        public const int Constructor = 0x1E050;
        public const int Destructor = 0x1E180;
        public const int Delete = 0x1E0A0;
        public const int Draw = 0x1E0E0;
        public const int Create = 0x1E1C0;
    }

    public static class CTextDraw
    {
        public const int Constructor = 0xB2E50;
        public const int Destructor = 0xB26C0;
        public const int SetText = 0xB26D0;
        public const int Draw = 0xB2BF0;
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

    public static class CEntity
    {
        public const int GetMatrix = 0x9E400;
        public const int SetMatrix = 0x9E4B0;
        public const int UpdateRwFrame = 0x9E570;
        public const int GetSpeed = 0x9E5D0;
        public const int SetSpeed = 0x9E600;
        public const int GetTurnSpeed = 0x9E720;
        public const int SetTurnSpeed = 0x9E750;
        public const int ApplyTurnSpeed = 0x9E780;
        public const int SetModelIndex = 0x9E840;
        public const int GetModelIndex = 0x9E920;
        public const int Teleport = 0x9E930;
        public const int GetDistanceToLocalPlayer = 0x9E9B0;
        public const int GetDistanceToCamera = 0x9EA80;
        public const int GetDistanceToLocalPlayerNoHeight = 0x9EAE0;
        public const int GetDistanceToPoint = 0x9EBA0;
        public const int DoesExist = 0x9ECC0;
        public const int GetCollisionFlag = 0x9EF50;
        public const int SetCollisionFlag = 0x9EF20;
        public const int SetCollisionProcessed = 0x9EF70;
        public const int GetEulerInverted = 0x9F1E0;
    }

    public static class CPed
    {
        public const int GetHealth = 0xAB4C0;
        public const int SetHealth = 0xAB4E0;
        public const int GetArmour = 0xAB500;
        public const int SetArmour = 0xAB520;
        public const int GetState = 0xAB5B0;
        public const int SetState = 0xAB5C0;
        public const int GetRotation = 0xAB600;
        public const int ForceRotation = 0xAB680;
        public const int SetRotation = 0xAB6D0;
        public const int IsPassenger = 0xAB730;
        public const int GetVehicle = 0xAB770;
        public const int GetCurrentWeapon = 0xAB3C0;
        public const int GetCurrentWeaponAmmo = 0xAC8C0;
        public const int GetVehicleSeatIndex = 0xAB970;
        public const int ClearWeapons = 0xAB780;
        public const int EnableJetpack = 0xAC480;
        public const int DisableJetpack = 0xAC4D0;
        public const int HasJetpack = 0xAC530;
        public const int GetAimZ = 0xAD060;
        public const int GetBonePosition = 0xADC00;
        public const int SetAimZ = 0xAD0A0;
        public const int HasAccessory = 0xAE5A0;
        public const int DeleteAccessory = 0xAE5C0;
        public const int GetAccessoryState = 0xAE620;
        public const int GetAccessory = 0x13330;
        public const int DeleteAllAccessories = 0xB0220;
        public const int AddAccessory = 0xB0280;
    }

    public static class CRemotePlayer
    {
        public const int GetDistanceToLocalPlayer = 0x15BB0;
        public const int SetColor = 0x15BE0;
        public const int GetColorAsRgba = 0x15C00;
        public const int GetColorAsArgb = 0x15C10;
        public const int GetStatus = 0x15DB0;
        public const int DoesExist = 0x1080;
    }

    public static class CVehicle
    {
        public const int HasDriver = 0xB6850;
        public const int IsOccupied = 0xB68A0;
        public const int SetInvulnerable = 0xB6900;
        public const int SetLocked = 0xB69A0;
        public const int GetHealth = 0xB6A10;
        public const int SetHealth = 0xB6A30;
        public const int SetColor = 0xB6A50;
        public const int UpdateColor = 0xB6AA0;
        public const int EnableSiren = 0xB6CB0;
        public const int SirenEnabled = 0xB6CD0;
        public const int GetTrailer = 0xB7400;
        public const int DoesExist = 0xB78A0;
        public const int SetLicensePlateText = 0xB78B0;
        public const int SetRotation = 0xB78D0;
        public const int EnableEngine = 0xB81D0;
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

        public const int Entries = 0x0132;
        public const int Redraw = 0x63DA;
        public const int Size = 25578;

        public const int EntryCount = 100;
        public const int EntrySize = 252;

        public const int Entry_Timestamp = 0x00;
        public const int Entry_Prefix = 0x04;
        public const int Entry_PrefixSize = 28;
        public const int Entry_Text = 0x20;
        public const int Entry_TextSize = 144;
        public const int Entry_Type = 0xF0;
        public const int Entry_TextColor = 0xF4;
        public const int Entry_PrefixColor = 0xF8;
    }

    public static class RakClientVTable
    {
        public const int EntryCount = 55;
        public const int Send_BitStream = 6;
        public const int Send_Data = 7;
        public const int Receive = 8;
    }

    public static class RakPeer
    {
        // Packet* __cdecl AllocPacket(unsigned int dataSize)
        public const int AllocPacket = 0x37B90;

        // Offset within RakPeer struct to the packet SingleProducerConsumer queue.
        public const int OffsetPackets = 0xDB6;

        // Packet** __thiscall WriteLock(void* queue) — acquires write slot.
        public const int WriteLock = 0x38EC0;

        // void __thiscall WriteUnlock(void* queue) — commits the write.
        public const int WriteUnlock = 0x38F00;
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
