namespace SFSharp.Interop.RakNet.Arizona.Enum;

// Arizona RP extended sub-packet IDs for Packet 221.
// Wire format used by the project:
//   [u8 packet_id=221] [u16 sub_id] [... payload ...]
// libcef.asi confirms that the Packet 221 payload is then decoded by the libPED RPC registry.
// Only IDs confirmed in libcef.asi are named here.
public enum EArizonaEx : ushort
{
    #region outgoing (client -> server)

    // u16 ped_id, vec3 position, byte[4] raw, bit-bool pad, float heading, byte[3] raw
    // current project parser matches observed client-origin packet shape
    BotSendOnfootSync = 53,

    // bit-bool give_or_take, u16 ped_id, float damage, u8 weapon_id, u8 bodypart, u16 unknown0, u16 unknown1
    // current project parser reflects observed outgoing shape; semantic names of trailing fields are not fully confirmed
    BotSendDamage = 73,

    #endregion

    #region incoming (server -> client)

    // u16 ped_id, i16 model_id, vec3 pos, float rot, bool pad,
    // float hp, float armour, byte[3] pad,
    // { i32 color, string32 text } nametag_1,
    // byte[1] pad,
    // { i32 color, string32 text } nametag_2,
    // byte[7] pad
    BotWorldPedAdd = 50,

    // u16 ped_id
    BotWorldPedRemove = 51,

    // u16 ped_id, byte[4] raw_sync, bit-bool padding, float health, float max_health, float armour, float max_armour, u8 trailing
    BotOnfootPedSync = 52,

    // u16 ped_id, u32 color, bit-bool trailing
    BotSetPedColor = 54,

    // u16 ped_id, u8 fight_style
    BotSetPedFightStyle = 55,

    // u16 ped_id, bit-bool invulnerable
    BotSetPedInvulnerable = 56,

    // u16 ped_id, string32 name, bit-bool trailing
    BotSetPedName = 57,

    // u16 ped_id, u16 unknown0, u16 skin_id
    BotSetPedSkin = 64,

    // u16 ped_id, u16 unknown0, u16 weapon_id, u8 trailing
    BotSetPedWeapon = 65,

    // u16 ped_id, vec3 position
    BotSetPedPos = 66,

    // u16 ped_id, vec3 position, u16 unknown0, u32 unknown1
    BotMovePedToPos = 67,

    // u16 ped_id, u16 unknown0, vec3 target_pos
    BotShootPedAtPos = 68,

    // u16 ped_id, string32 anim_lib, string32 anim_name, float blend_delta, 4 bit-bool flags, u32 duration_ms
    BotApplyPedAnimation = 69,

    // u16 ped_id, u16 unknown0
    BotClearPedAction = 70,

    // u16 ped_id, u16 unknown0, u16 player_id
    BotShootPedAtPlayer = 72,

    // u16 ped_id, u16 unknown0, u16 player_id, u32 unknown1
    BotAttackPlayer = 80,

    // u16 ped_id, u16 vehicle_id, u16 seat_id, u32 unknown0
    BotEnterToVehicle = 81,

    // u16 ped_id, u16 vehicle_id, u16 seat_id, float health, float armour
    BotPassengerPedSync = 82,

    // u16 ped_id, u16 vehicle_id, u16 unknown0, u32 state0, u32 state1
    BotDrivePedSync = 83,

    // u16 ped_id
    BotRemoveFromVehicle = 84,

    // u16 ped_id, string32 text, i32 color, float distance, i32 duration
    BotChatBubble = 85,

    // u16 ped_id, u16 slot, i32 model_id, i16 bone_id, vec3 offset, vec3 rotation, vec3 scale, i32 color1, i32 color2
    BotAttachObject = 86,

    // u16 ped_id, u16 slot
    BotDetachObject = 87,

    // u16 ped_id, u16 unknown0, float angle
    BotSetPedAngle = 89,

    // u16 ped_id, u16 unknown0
    BotStopAllAction = 96,

    // u16 shooter_ped_id, u16 target_ped_id, u16 unknown0
    BotShootPedAtPed = 97,

    // u16 ped_id, u16 unknown0, string8 animation_group
    BotSetAnimationGroup = 98,

    // u16 ped_id, u16 target_ped_id, u32 unknown0
    BotAttackPed = 101,

    // u16 ped_id, bool state
    BotToggleCollision = 102,

    // same layout as id 86 plus one trailing bit-bool flag
    BotAttachSimpleObject = 103,

    // u16 ped_id, u16 slot
    BotDetachSimpleObject = 104,

    // u16 ped_id, u16 unknown0, u32 status, float current_value, float max_value
    BotSetHealth = 105,

    // u16 ped_id, u16 unknown0, u32 status, float current_value, float max_value
    BotSetArmour = 112,

    // u16 onfoot_sync_rate
    BotSetOnfootSyncRate = 113,

    #endregion
}
