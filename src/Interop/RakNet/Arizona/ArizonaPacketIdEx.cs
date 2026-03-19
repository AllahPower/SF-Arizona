namespace SFSharp;

// Arizona RP extended sub-packet IDs carried inside raw Packet 221 (ArizonaCefEx).
// Transport: first 2 bytes of Packet 221 payload = this enum value (uint16).
// Remaining bytes = sub-packet payload parsed per-ID.
// Used primarily for the bot/NPC system.
public enum ArizonaPacketIdEx : ushort
{
    // -- incoming (server -> client) --

    // u16 bot_id, i16 model_id, vec3 pos, float rot, bool pad,
    // float hp, float armour, byte[3] pad,
    // { i32 color, string32 text } nametag_1,
    // byte[1] pad,
    // { i32 color, string32 text } nametag_2,
    // byte[7] pad
    BotStreamIn = 50,

    // u16 bot_id
    BotStreamOut = 51,

    // u16 bot_id, byte[4] pad, bool pad, float hp, float max_hp, float armour, float max_armour, byte[1] pad
    BotOnfootSync = 52,

    // u16 bot_id, bool invulnerable
    SetBotInvulnerable = 56,

    // u16 bot_id, string32 name, u8 unused
    SetBotName = 57,

    // u16 bot_id, u16 weapon_id, u8 unknown
    SetBotWeapon = 65,

    // u16 bot_id, vec3 position
    SetBotPos = 66,

    // u16 bot_id, vec3 position, u16 unknown, u32 unknown
    MoveBotToPos = 67,

    // u16 bot_id, string32 anim_lib, string32 anim_name, byte[9] unknown
    ApplyBotAnimation = 69,

    // u16 bot_id, u16 player_id, u32 unknown
    BotAttackPlayer = 80,

    // u16 bot_id, u16 vehicle_id, i16 unknown, u32 unknown
    BotEnterVehicle = 81,

    // u16 bot_id, u16 vehicle_id, i16 seat_id, float hp, float armour
    BotPassengerSync = 82,

    // u16 bot_id
    BotExitVehicle = 84,

    // u16 bot_id, string32 text, i32 color, float distance, i32 duration
    BotChatBubble = 85,

    // u16 bot_id, u16 slot, i32 model_id, i16 bone_id, vec3 offset, vec3 rotation, vec3 scale, i32 color1, i32 color2
    SetBotAttachedObject = 86,

    // u16 bot_id, u16 slot
    RemoveBotAttachedObject = 87,

    // u16 shooter_bot_id, u16 target_bot_id
    ShootBotAtBot = 97,

    // u16 bot_id, bool8 unknown
    DestroyBot = 102,

    // u16 bot_id, u16 slot, i32 model_id, i16 bone_id, vec3 offset, vec3 rotation, vec3 scale, i32 color1, i32 color2
    SetBotAttachedSimpleObject = 103,

    // -- outgoing (client -> server) --

    // u16 bot_id, vec3 position, byte[4] pad, bool pad, float heading, byte[3] pad
    SendBotOnfootSync = 53,

    // bool give_or_take, u16 bot_id, float damage, i32 weapon, i32 bodypart, u16 unknown, u16 player_id
    SendBotDamage = 73,
}
