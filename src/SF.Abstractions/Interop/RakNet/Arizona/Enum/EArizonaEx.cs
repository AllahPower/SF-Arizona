namespace SFSharp.Abstractions.Interop.RakNet;

/// <summary>
/// Arizona RP extended sub-packet identifiers for packet 221.
/// </summary>
/// <remarks>
/// Assert: the observed wire format is `[u8 packetId=221] [u16 subId] [...payload...]`.
/// Assert: libcef.asi confirms that packet 221 payloads are decoded by the libPED RPC registry.
/// Assert: only IDs confirmed in libcef.asi are named here.
/// </remarks>
public enum EArizonaEx : ushort
{
    #region outgoing (client -> server)

    // Assert: u16 ped_id, vec3 position, byte[4] raw, bit-bool pad, float heading, byte[3] raw.
    // Assert: current project parser matches the observed client-origin packet shape.
    BotSendOnfootSync = 53,

    // Assert: bit-bool give_or_take, u16 ped_id, float damage, u8 weapon_id, u8 bodypart, u16 unknown0, u16 unknown1.
    // Assert: trailing field semantics are not fully confirmed yet.
    BotSendDamage = 73,

    #endregion

    #region incoming (server -> client)

    // Assert: u16 ped_id, i16 model_id, vec3 pos, float rot, bool pad, float hp, float armour, byte[3] pad,
    // Assert: nametag_1 { i32 color, string32 text }, byte[1] pad, nametag_2 { i32 color, string32 text }, byte[7] pad.
    BotWorldPedAdd = 50,

    // Assert: u16 ped_id.
    BotWorldPedRemove = 51,

    // Assert: u16 ped_id, byte[4] raw_sync, bit-bool padding, float health, float max_health, float armour, float max_armour, u8 trailing.
    BotOnfootPedSync = 52,

    // Assert: u16 ped_id, u32 color, bit-bool trailing.
    BotSetPedColor = 54,

    // Assert: u16 ped_id, u8 fight_style.
    BotSetPedFightStyle = 55,

    // Assert: u16 ped_id, bit-bool invulnerable.
    BotSetPedInvulnerable = 56,

    // Assert: u16 ped_id, string32 name, bit-bool trailing.
    BotSetPedName = 57,

    // Assert: u16 ped_id, u16 unknown0, u16 skin_id.
    BotSetPedSkin = 64,

    // Assert: u16 ped_id, u16 unknown0, u16 weapon_id, u8 trailing.
    BotSetPedWeapon = 65,

    // Assert: u16 ped_id, vec3 position.
    BotSetPedPos = 66,

    // Assert: u16 ped_id, vec3 position, u16 unknown0, u32 unknown1.
    BotMovePedToPos = 67,

    // Assert: u16 ped_id, u16 unknown0, vec3 target_pos.
    BotShootPedAtPos = 68,

    // Assert: u16 ped_id, string32 anim_lib, string32 anim_name, float blend_delta, four bit-bool flags, u32 duration_ms.
    BotApplyPedAnimation = 69,

    // Assert: u16 ped_id, u16 unknown0.
    BotClearPedAction = 70,

    // Assert: u16 ped_id, u16 unknown0, u16 player_id.
    BotShootPedAtPlayer = 72,

    // Assert: visible libPED RPC_AttackPlayer handler resolves the acting bot from the second u16 field.
    // Assert: the third u16 is passed as target player id and the trailing u32 is forwarded as an attack argument/state.
    // Assert: live traffic was also observed at 64 bits, so the trailing u32 is treated as optional until the hidden/runtime path is fully recovered.
    BotAttackPlayer = 80,

    // Assert: visible libPED RPC_EnterToVehicle handler resolves the acting bot from the second u16 field.
    // Assert: the third u16 is passed as vehicle id, the fourth u16 as seat id, and the trailing u32 as an extra enter-vehicle argument/state.
    BotEnterToVehicle = 81,

    // Assert: visible libPED RPC_PassengerPedSync handler resolves the acting bot from the second u16 field.
    // Assert: the third u16 is used as vehicle id; the low 16 bits of the first u32 are used as passenger seat id.
    // Assert: the remaining upper bits / trailing u32 are still unresolved.
    BotPassengerPedSync = 82,

    // Assert: visible libPED RPC_DrivePedSync handler resolves the acting bot from the second u16 field.
    // Assert: the third u16 is used as vehicle id and the bot is forced into driver seat 0.
    // Assert: both trailing u32 state fields are still unresolved.
    BotDrivePedSync = 83,

    // Assert: u16 ped_id.
    BotRemoveFromVehicle = 84,

    // Assert: u16 ped_id, string32 text, i32 color, float distance, i32 duration.
    BotChatBubble = 85,

    // Assert: u16 ped_id, u16 slot, i32 model_id, i16 bone_id, vec3 offset, vec3 rotation, vec3 scale, i32 color1, i32 color2.
    BotAttachObject = 86,

    // Assert: u16 ped_id, u16 slot.
    BotDetachObject = 87,

    // Assert: u16 ped_id, u16 unknown0, float angle.
    BotSetPedAngle = 89,

    // Assert: u16 ped_id, u16 unknown0.
    BotStopAllAction = 96,

    // Assert: u16 shooter_ped_id, u16 target_ped_id, u16 unknown0.
    BotShootPedAtPed = 97,

    // Assert: u16 ped_id, u16 unknown0, string8 animation_group.
    BotSetAnimationGroup = 98,

    // Assert: visible libPED RPC_AttackPed handler resolves the acting bot from the second u16 field.
    // Assert: the third u16 is passed as target bot/ped id and the trailing u32 is forwarded as an attack argument/state.
    BotAttackPed = 101,

    // Assert: u16 ped_id, bool state.
    BotToggleCollision = 102,

    // Assert: same layout as id 86 plus one trailing bit-bool flag.
    BotAttachSimpleObject = 103,

    // Assert: visible libPED RPC_DetachSimpleObject class reads u16 ped_id, u16 slot, u16 unknown0.
    BotDetachSimpleObject = 104,

    // Assert: live traffic is observed as 112 bits, not 128 bits.
    // Assert: current provisional shape is u16 ped_id, u16 unknown0, u32 status, float current_value, u16 trailing_value.
    // Assert: a visible libPED RPC registry handler for 105 is still not located in the current binary.
    BotSetHealth = 105,

    // Assert: live traffic is observed as 112 bits, not 128 bits.
    // Assert: current provisional shape is u16 ped_id, u16 unknown0, u32 status, float current_value, u16 trailing_value.
    // Assert: a visible libPED RPC registry handler for 112 is still not located in the current binary.
    BotSetArmour = 112,

    // Assert: u16 onfoot_sync_rate.
    BotSetOnfootSyncRate = 113,

    #endregion
}
