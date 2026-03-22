using System.Numerics;

namespace SFSharp;

// --- Packet 221 incoming (server -> client) --- bot/NPC system ---

public readonly record struct ArzBotNametag(int Color, string Text);

// id=50 | bot appears in streaming range
public readonly record struct ArzBotStreamIn(
    ushort BotId,
    short ModelId,
    Vector3 Position,
    float Rotation,
    bool Padding,
    float Health,
    float Armour,
    ushort UnknownShort0,
    byte UnknownByte0,
    ArzBotNametag Nametag1,
    byte UnknownByte1,
    ArzBotNametag Nametag2,
    byte[] UnknownTail
);

// id=51 | bot leaves streaming range
public readonly record struct ArzBotStreamOut(ushort BotId);

// id=52 | on-foot bot sync; after internal rpcId, payload matches bot_id + raw4 + bit + 4 floats + raw1
public readonly record struct ArzBotOnfootSync(
    ushort BotId,
    uint Unknown1Raw,
    bool Padding,
    float Health,
    float MaxHealth,
    float Armour,
    float MaxArmour,
    byte Unknown2
);

// id=54 | set bot color/state
public readonly record struct ArzSetBotColor(ushort BotId, uint Color, bool Unknown0);

// id=55 | set bot fight style
public readonly record struct ArzSetBotFightStyle(ushort BotId, byte FightStyle);

// id=56 | bot invulnerability flag
public readonly record struct ArzSetBotInvulnerable(ushort BotId, bool Invulnerable);

// id=57 | rename a bot
public readonly record struct ArzSetBotName(ushort BotId, string Name, bool Unknown0);

// id=64 | set bot skin/model
public readonly record struct ArzSetBotSkin(ushort BotId, ushort Unknown0, ushort SkinId);

// id=65 | set bot held weapon
public readonly record struct ArzSetBotWeapon(ushort BotId, ushort Unknown0, ushort WeaponId, byte Unknown1);

// id=66 | teleport bot to position
public readonly record struct ArzSetBotPos(ushort BotId, Vector3 Position);

// id=67 | move bot smoothly to position
public readonly record struct ArzMoveBotToPos(ushort BotId, Vector3 Position, ushort Unknown0, uint Unknown1);

// id=68 | force bot aim/fire at world position
public readonly record struct ArzShootBotAtPos(ushort BotId, ushort Unknown0, Vector3 TargetPosition);

// id=69 | play animation on bot
public readonly record struct ArzApplyBotAnimation(
    ushort BotId,
    string AnimLib,
    string AnimName,
    float BlendDelta,
    bool Loop,
    bool LockX,
    bool LockY,
    bool Freeze,
    uint DurationMs
);

// id=70 | clear current bot action/state
public readonly record struct ArzClearBotAction(ushort BotId, ushort Unknown0);

// id=72 | force bot aim/fire at player
public readonly record struct ArzShootBotAtPlayer(ushort BotId, ushort Unknown0, ushort PlayerId);

// id=80 | bot attacks a player
public readonly record struct ArzBotAttackPlayer(ushort BotId, ushort Unknown0, ushort PlayerId, uint Unknown1);

// id=81 | bot enters a vehicle
public readonly record struct ArzBotEnterVehicle(ushort BotId, ushort VehicleId, ushort SeatId, ushort Unknown0, uint Unknown1);

// id=82 | bot riding as passenger sync
public readonly record struct ArzBotPassengerSync(ushort BotId, ushort VehicleId, ushort SeatId, ushort Unknown0, float Health, float Armour);

// id=83 | bot driving vehicle sync
public readonly record struct ArzBotDriveSync(ushort BotId, ushort VehicleId, ushort Unknown0, float Health, float Armour);

// id=84 | bot exits vehicle
public readonly record struct ArzBotExitVehicle(ushort BotId, ushort Unknown0);

// id=85 | chat bubble above bot
public readonly record struct ArzBotChatBubble(ushort BotId, string Text, int Color, float Distance, int Duration);

// id=86 | attach object to bot
public readonly record struct ArzSetBotAttachedObject(
    ushort BotId, ushort Slot,
    int ModelId, short BoneId,
    Vector3 Offset, Vector3 Rotation, Vector3 Scale,
    int Color1, int Color2
);

// id=87 | detach object from bot
public readonly record struct ArzRemoveBotAttachedObject(ushort BotId, ushort Slot, ushort Unknown0);

// id=89 | set bot facing angle
public readonly record struct ArzSetBotAngle(ushort BotId, ushort Unknown0, float Angle);

// id=96 | stop all bot actions
public readonly record struct ArzStopBotAction(ushort BotId, ushort Unknown0);

// id=97 | bot shoots at another bot
public readonly record struct ArzShootBotAtBot(ushort ShooterBotId, ushort TargetBotId, ushort Unknown0);

// id=98 | assign named animation group to bot
public readonly record struct ArzSetBotAnimationGroup(ushort BotId, ushort Unknown0, string GroupName);

// id=102 | collision toggle for a remote ped
public readonly record struct ArzTogglePedCollision(ushort BotId, bool State);

// id=103 | attach simple object to bot (same layout as id=86)
public readonly record struct ArzSetBotAttachedSimpleObject(
    ushort BotId, ushort Slot,
    int ModelId, short BoneId,
    Vector3 Offset, Vector3 Rotation, Vector3 Scale,
    int Color1, int Color2,
    bool Unknown0
);

// id=104 | detach simple object from bot
public readonly record struct ArzRemoveBotAttachedSimpleObject(ushort BotId, ushort Slot, ushort Unknown0);

// id=105 | set bot health values
public readonly record struct ArzSetBotHealth(ushort BotId, ushort Unknown0, uint Status, float CurrentValue, float MaximumValue);

// id=112 | set bot armour values
public readonly record struct ArzSetBotArmour(ushort BotId, ushort Unknown0, uint Status, float CurrentValue, float MaximumValue);

// id=113 | set bot settings/mask
public readonly record struct ArzSetBotSettings(ushort BotId, ushort SettingsMask);

// --- Packet 221 outgoing (client -> server) ---

// id=53 | client-side bot position sync
public readonly record struct ArzSendBotOnfootSync(ushort BotId, Vector3 Position, float Heading);

// id=73 | bot damage report
// first flag is observed as a single-bit value; remaining fields are still being recovered from core.asi
public readonly record struct ArzSendBotDamage(
    bool GiveOrTake,
    ushort BotId,
    float Damage,
    byte WeaponId,
    byte BodyPart,
    ushort Unknown0,
    ushort Unknown1
);

// ============================================================================
// Parser methods.
// Packet 220 parsers start after the sub-id byte.
// Packet 221 parsers start after the u16 sub-id in the current transport layer.
// ============================================================================


