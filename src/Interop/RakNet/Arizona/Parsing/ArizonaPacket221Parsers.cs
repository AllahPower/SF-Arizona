using System.Numerics;
using System.Text;
using SFSharp.Interop.RakNet.Arizona.Enum;

namespace SFSharp;

public static partial class ArizonaPacket
{
    private static void ExpectBotRpcId(ref BitStreamReader r, EArizonaEx expectedSubId)
    {
        // Packet 221 readers are already positioned after sub-id in the current parser pipeline.
    }

    // ---- Packet 221 incoming parsers (bot system) ----

    public static ArzBotStreamIn ParseBotStreamIn(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotWorldPedAdd);
        ushort botId = r.ReadUInt16();
        short modelId = r.ReadInt16();
        Vector3 position = ReadVec3(ref r);
        float rotation = r.ReadFloat();
        bool padding = r.ReadBitBool();
        float health = r.ReadFloat();
        float armour = r.ReadFloat();
        ushort unknownShort0 = r.ReadUInt16();
        byte unknownByte0 = r.ReadUInt8();
        ArzBotNametag nametag1 = new(r.ReadInt32(), r.ReadStringUInt32Length());
        byte unknownByte1 = r.ReadUInt8();
        ArzBotNametag nametag2 = new(r.ReadInt32(), r.ReadStringUInt32Length());
        byte[] unknownTail = r.ReadBytes(Math.Min(7, r.LengthBytes - (r.OffsetBits + 7) / 8)).ToArray();

        return new(
            botId,
            modelId,
            position,
            rotation,
            padding,
            health,
            armour,
            unknownShort0,
            unknownByte0,
            nametag1,
            unknownByte1,
            nametag2,
            unknownTail);
    }

    public static ArzBotStreamOut ParseBotStreamOut(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotWorldPedRemove);
        return new(r.ReadUInt16());
    }

    public static ArzBotOnfootSync ParseBotOnfootSync(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotOnfootPedSync);
        ushort botId = r.ReadUInt16();
        uint unknown1Raw = r.ReadUInt32();
        bool padding = r.ReadBitBool();
        float health = r.ReadFloat();
        float maxHealth = r.ReadFloat();
        float armour = r.ReadFloat();
        float maxArmour = r.ReadFloat();
        byte unknown2 = r.RemainingBits >= 8 ? r.ReadUInt8() : (byte)0;
        return new(botId, unknown1Raw, padding, health, maxHealth, armour, maxArmour, unknown2);
    }

    public static ArzSetBotColor ParseSetBotColor(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetPedColor);
        ushort botId = r.ReadUInt16();
        uint color = r.ReadUInt32();
        bool unknown0 = r.ReadBitBool();
        return new(botId, color, unknown0);
    }

    public static ArzSetBotFightStyle ParseSetBotFightStyle(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetPedFightStyle);
        ushort botId = r.ReadUInt16();
        byte fightStyle = r.ReadUInt8();
        return new(botId, fightStyle);
    }

    public static ArzSetBotInvulnerable ParseSetBotInvulnerable(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetPedInvulnerable);
        ushort botId = r.ReadUInt16();
        bool invuln = r.ReadBitBool();
        return new(botId, invuln);
    }

    public static ArzSetBotName ParseSetBotName(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetPedName);
        ushort botId = r.ReadUInt16();
        string name = r.ReadStringUInt32Length();
        bool unknown0 = r.ReadBitBool();
        return new(botId, name, unknown0);
    }

    public static ArzSetBotSkin ParseSetBotSkin(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetPedSkin);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        ushort skinId = r.ReadUInt16();
        return new(botId, unknown0, skinId);
    }

    public static ArzSetBotWeapon ParseSetBotWeapon(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetPedWeapon);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        ushort weaponId = r.ReadUInt16();
        byte unknown1 = r.ReadUInt8();
        return new(botId, unknown0, weaponId, unknown1);
    }

    public static ArzSetBotPos ParseSetBotPos(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetPedPos);
        ushort botId = r.ReadUInt16();
        Vector3 pos = ReadVec3(ref r);
        return new(botId, pos);
    }

    public static ArzMoveBotToPos ParseMoveBotToPos(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotMovePedToPos);
        ushort botId = r.ReadUInt16();
        Vector3 pos = ReadVec3(ref r);
        ushort unknown0 = r.ReadUInt16();
        uint unknown1 = r.ReadUInt32();
        return new(botId, pos, unknown0, unknown1);
    }

    public static ArzShootBotAtPos ParseShootBotAtPos(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotShootPedAtPos);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        Vector3 targetPosition = ReadVec3(ref r);
        return new(botId, unknown0, targetPosition);
    }

    public static ArzApplyBotAnimation ParseApplyBotAnimation(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotApplyPedAnimation);
        ushort botId = r.ReadUInt16();
        string animLib = r.ReadStringUInt32Length();
        string animName = r.ReadStringUInt32Length();
        float blendDelta = r.ReadFloat();
        bool loop = r.ReadBitBool();
        bool lockX = r.ReadBitBool();
        bool lockY = r.ReadBitBool();
        bool freeze = r.ReadBitBool();
        uint durationMs = r.ReadUInt32();
        return new(botId, animLib, animName, blendDelta, loop, lockX, lockY, freeze, durationMs);
    }

    public static ArzClearBotAction ParseClearBotAction(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotClearPedAction);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        return new(botId, unknown0);
    }

    public static ArzShootBotAtPlayer ParseShootBotAtPlayer(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotShootPedAtPlayer);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        ushort playerId = r.ReadUInt16();
        return new(botId, unknown0, playerId);
    }

    public static ArzBotAttackPlayer ParseBotAttackPlayer(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotAttackPlayer);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        ushort playerId = r.ReadUInt16();
        uint unknown1 = r.ReadUInt32();
        return new(botId, unknown0, playerId, unknown1);
    }

    public static ArzBotEnterVehicle ParseBotEnterVehicle(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotEnterToVehicle);
        ushort botId = r.ReadUInt16();
        ushort vid = r.ReadUInt16();
        ushort seatId = r.ReadUInt16();
        uint unknown0 = r.ReadUInt32();
        return new(botId, vid, seatId, unknown0);
    }

    public static ArzBotPassengerSync ParseBotPassengerSync(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotPassengerPedSync);
        ushort botId = r.ReadUInt16();
        ushort vehicleId = r.ReadUInt16();
        ushort seatId = r.ReadUInt16();
        float health = r.ReadFloat();
        float armour = r.ReadFloat();
        return new(botId, vehicleId, seatId, health, armour);
    }

    public static ArzBotDriveSync ParseBotDriveSync(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotDrivePedSync);
        ushort botId = r.ReadUInt16();
        ushort vehicleId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        uint stateValue0 = r.ReadUInt32();
        uint stateValue1 = r.ReadUInt32();
        return new(botId, vehicleId, unknown0, stateValue0, stateValue1);
    }

    public static ArzBotExitVehicle ParseBotExitVehicle(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotRemoveFromVehicle);
        ushort botId = r.ReadUInt16();
        return new(botId);
    }

    public static ArzBotChatBubble ParseBotChatBubble(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotChatBubble);
        ushort botId = r.ReadUInt16();
        string text = r.ReadStringUInt32Length();
        int color = r.ReadInt32();
        float dist = r.ReadFloat();
        int duration = r.ReadInt32();
        return new(botId, text, color, dist, duration);
    }

    public static ArzSetBotAttachedObject ParseSetBotAttachedObject(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotAttachObject);
        ushort botId = r.ReadUInt16();
        ushort slot = r.ReadUInt16();
        int modelId = r.ReadInt32();
        short boneId = (short)r.ReadUInt16();
        Vector3 offset = ReadVec3(ref r);
        Vector3 rotation = ReadVec3(ref r);
        Vector3 scale = ReadVec3(ref r);
        int c1 = r.ReadInt32();
        int c2 = r.ReadInt32();
        return new(botId, slot, modelId, boneId, offset, rotation, scale, c1, c2);
    }

    public static ArzRemoveBotAttachedObject ParseRemoveBotAttachedObject(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotDetachObject);
        ushort botId = r.ReadUInt16();
        ushort slot = r.ReadUInt16();
        return new(botId, slot);
    }

    public static ArzSetBotAngle ParseSetBotAngle(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetPedAngle);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        float angle = r.ReadFloat();
        return new(botId, unknown0, angle);
    }

    public static ArzStopBotAction ParseStopBotAction(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotStopAllAction);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        return new(botId, unknown0);
    }

    public static ArzShootBotAtBot ParseShootBotAtBot(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotShootPedAtPed);
        ushort shooter = r.ReadUInt16();
        ushort target = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        return new(shooter, target, unknown0);
    }

    public static ArzSetBotAnimationGroup ParseSetBotAnimationGroup(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetAnimationGroup);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        string groupName = r.ReadStringUInt8Length();
        return new(botId, unknown0, groupName);
    }

    public static ArzBotAttackPed ParseBotAttackPed(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotAttackPed);
        ushort botId = r.ReadUInt16();
        ushort targetBotId = r.ReadUInt16();
        uint unknown0 = r.ReadUInt32();
        return new(botId, targetBotId, unknown0);
    }

    public static ArzTogglePedCollision ParseTogglePedCollision(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotToggleCollision);
        ushort botId = r.ReadUInt16();
        bool state = r.ReadUInt8() != 0;
        return new(botId, state);
    }

    public static ArzSetBotAttachedSimpleObject ParseSetBotAttachedSimpleObject(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotAttachSimpleObject);
        ushort botId = r.ReadUInt16();
        ushort slot = r.ReadUInt16();
        int modelId = r.ReadInt32();
        short boneId = (short)r.ReadUInt16();
        Vector3 offset = ReadVec3(ref r);
        Vector3 rotation = ReadVec3(ref r);
        Vector3 scale = ReadVec3(ref r);
        int c1 = r.ReadInt32();
        int c2 = r.ReadInt32();
        bool unknown0 = r.ReadBitBool();
        return new(botId, slot, modelId, boneId, offset, rotation, scale, c1, c2, unknown0);
    }

    public static ArzRemoveBotAttachedSimpleObject ParseRemoveBotAttachedSimpleObject(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotDetachSimpleObject);
        ushort botId = r.ReadUInt16();
        ushort slot = r.ReadUInt16();
        return new(botId, slot);
    }

    public static ArzSetBotHealth ParseSetBotHealth(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetHealth);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        uint status = r.ReadUInt32();
        float currentValue = r.ReadFloat();
        float maximumValue = r.ReadFloat();
        return new(botId, unknown0, status, currentValue, maximumValue);
    }

    public static ArzSetBotArmour ParseSetBotArmour(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetArmour);
        ushort botId = r.ReadUInt16();
        ushort unknown0 = r.ReadUInt16();
        uint status = r.ReadUInt32();
        float currentValue = r.ReadFloat();
        float maximumValue = r.ReadFloat();
        return new(botId, unknown0, status, currentValue, maximumValue);
    }

    public static ArzSetBotOnfootSyncRate ParseSetBotOnfootSyncRate(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSetOnfootSyncRate);
        ushort rate = r.ReadUInt16();
        return new(rate);
    }

    // ---- Packet 221 outgoing parsers ----

    public static ArzSendBotOnfootSync ParseSendBotOnfootSync(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSendOnfootSync);
        ushort botId = r.ReadUInt16();
        Vector3 pos = ReadVec3(ref r);
        r.SkipBytes(4); // unknown
        r.SkipBits(1);  // bool padding
        float heading = r.ReadFloat();
        r.SkipBytes(3); // trailing
        return new(botId, pos, heading);
    }

    public static ArzSendBotDamage ParseSendBotDamage(ref BitStreamReader r)
    {
        ExpectBotRpcId(ref r, EArizonaEx.BotSendDamage);
        bool giveOrTake = r.ReadBitBool();
        ushort botId = r.ReadUInt16();
        float damage = r.ReadFloat();
        byte weaponId = r.ReadUInt8();
        byte bodyPart = r.ReadUInt8();
        ushort unknown0 = r.ReadUInt16();
        ushort unknown1 = r.ReadUInt16();
        return new(giveOrTake, botId, damage, weaponId, bodyPart, unknown0, unknown1);
    }

}


