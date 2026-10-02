using System.Numerics;
using System.Text;

namespace SFSharp.Runtime.Networking.RakNet.Arizona;

public static partial class ArizonaPacket
{
    // ---- Packet 220 outgoing parsers ----

    public static ArzSendKey ParseSendKey(ref SampBitStreamReader r)
    {
        byte key = r.ReadUInt8();
        byte unknown = r.ReadUInt8();
        return new(key, unknown);
    }

    public static ArzSendSwitchChatState ParseSendSwitchChatState(ref SampBitStreamReader r)
    {
        return new(r.ReadBool8());
    }

    public static ArzSendTurnLights ParseSendTurnLights(ref SampBitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzInjectCodeResponse ParseInjectCodeResponse(ref SampBitStreamReader r)
    {
        uint browserId = r.ReadUInt32();
        uint requestId = r.ReadUInt32();
        return new(browserId, requestId);
    }

    public static ArzSendText ParseSendText(ref SampBitStreamReader r)
    {
        string text = r.ReadStringUInt16Length();
        uint sid = r.ReadUInt32();
        return new(text, sid);
    }

    public static ArzModuleReadResponse ParseModuleReadResponse(ref SampBitStreamReader r)
    {
        uint moduleOffset = r.ReadUInt32();
        byte moduleNameLength = r.ReadUInt8();
        string moduleName = moduleNameLength == 0
            ? string.Empty
            : Encoding.ASCII.GetString(r.ReadBytes(moduleNameLength).ToArray());
        byte status = r.ReadUInt8();
        byte[] data = status == 0 && r.RemainingBits > 0
            ? r.ReadBytes((r.RemainingBits + 7) / 8).ToArray()
            : [];
        return new(moduleOffset, moduleName, status, data);
    }

    public static ArzBrowserControlStateReply ParseBrowserControlStateReply(ref SampBitStreamReader r)
    {
        uint browserId = r.ReadUInt32();
        bool state = r.ReadBitBool();
        return new(browserId, state);
    }

    public static ArzSendHWID ParseSendHWID(ref SampBitStreamReader r)
    {
        byte[] hexDigestBytes = r.ReadRemainingBytes();
        return new(hexDigestBytes);
    }

    public static ArzSendVehicleSpeedLimiterState ParseSendVehicleSpeedLimiterState(ref SampBitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendSwitchChatMode ParseSendSwitchChatMode(ref SampBitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendSrcursorPosition ParseSendSrcursorPosition(ref SampBitStreamReader r)
    {
        float x = r.ReadFloat();
        float y = r.ReadFloat();
        return new(x, y);
    }

    public static ArzInCarNanCheckReport ParseInCarNanCheckReport(ref SampBitStreamReader r)
    {
        byte reportKind = r.ReadUInt8();
        ushort vehicleId = r.ReadUInt16();
        return new(reportKind, vehicleId);
    }

    public static ArzSendKeyboardLayoutCapsState ParseSendKeyboardLayoutCapsState(ref SampBitStreamReader r)
    {
        byte keyboardLayoutLowByte = r.ReadUInt8();
        bool capsLockOn = r.ReadBitBool();
        return new(keyboardLayoutLowByte, capsLockOn);
    }

    public static ArzSendFloatValue ParseSendFloatValue(ref SampBitStreamReader r)
    {
        return new(r.ReadFloat());
    }

    public static ArzSendToggleActionState ParseSendToggleActionState(ref SampBitStreamReader r)
    {
        return new(r.ReadBool8());
    }

    public static ArzSendTargetPosition ParseSendTargetPosition(ref SampBitStreamReader r)
    {
        return new(ReadVec3(ref r));
    }

    public static ArzSendSimpleTuningProgress ParseSendSimpleTuningProgress(ref SampBitStreamReader r)
    {
        uint accumulatedValue = r.ReadUInt32();
        byte tierStep = r.ReadUInt8();
        return new(accumulatedValue, tierStep);
    }

    public static ArzSendCommandLine ParseSendCommandLine(ref SampBitStreamReader r)
    {
        return new(r.ReadStringUInt16Length());
    }

    public static ArzSendDroneHeading ParseSendDroneHeading(ref SampBitStreamReader r)
    {
        return new(r.ReadFloat());
    }

    public static ArzSendNavigationArrowSelection ParseSendNavigationArrowSelection(ref SampBitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendPortalToggle ParseSendPortalToggle(ref SampBitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendPortalPlacementPreview ParseSendPortalPlacementPreview(ref SampBitStreamReader r)
    {
        byte portalType = r.ReadUInt8();
        Vector3 pointA = ReadVec3(ref r);
        Vector3 pointB = ReadVec3(ref r);
        return new(portalType, pointA, pointB);
    }

    public static ArzSendWeaponScroll ParseSendWeaponScroll(ref SampBitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendDamageResponseWeapon ParseSendDamageResponseWeapon(ref SampBitStreamReader r)
    {
        return new(r.ReadUInt8());
    }
}
