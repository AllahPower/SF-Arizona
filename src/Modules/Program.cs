using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Linq;
using SFSharp;

public static class Program
{
    public static async void Main()
    {
        SFLog.Info("Program.Main started");

        string version = typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        SF.Chat.Add($"{{00FF00}}SF-Arizona {{FFFFFF}}v{version}");
        SF.Chat.Add("{95FF4F}github.com/AllahPower/SF-Arizona | by AllahPower");

        var container = new SFModuleContainer();
        container.RegisterModule<DialogScraper>();
        container.RegisterModule<BrightBinder>();
        container.RegisterModule<LicenseShooter>();
        container.RegisterModule<NodShaker>();
        container.RegisterModule<ChatViolationMonitor>();
        container.RegisterModule<RpcDebugger>();
        container.RegisterModule<DebugModule>();

        using var debugCommand = SF.Chat.RegisterChatCommand("sfd", _ =>
        {
            SFLog.Info("Debug command /sfd executed");

            if (!CArizonaChat.IsAvailable)
            {
                SF.Chat.Add("Arizona chat runtime is not available.");
                return;
            }

            if (!CArizonaChat.TryGetRooms(out ArizonaChatRoomSnapshot[] rooms))
            {
                SF.Chat.Add("Arizona chat runtime is available, but room snapshot could not be read.");
                return;
            }

            if (CArizonaChat.TryGetActiveRoom(out ArizonaChatRoomSnapshot activeRoom))
            {
                SF.Chat.Add($"Active room: idx={activeRoom.RoomIndex} type={activeRoom.Type} name={activeRoom.Name}");
            }
            else
            {
                SF.Chat.Add("Active room: <unresolved>");
            }

            if (rooms.Length == 0)
            {
                SF.Chat.Add("Arizona rooms: none");
                return;
            }

            ArizonaChatRoomSnapshot[] visibleRooms = rooms
                .Where(static room => !room.IsHiddenByConfig && !room.IsInactive)
                .OrderBy(static room => room.RoomIndex)
                .ToArray();

            SF.Chat.Add($"Arizona visible rooms: {visibleRooms.Length} / total={rooms.Length}");
            foreach (ArizonaChatRoomSnapshot room in visibleRooms)
            {
                string flags = $"active={room.IsActive} hidden={room.IsHiddenByConfig} inactive={room.IsInactive} muteOverride={room.HasMuteOverride} muted={room.IsMuted}";
                string dynamicInfo = room.IsDynamic
                    ? $" dynamic roomId={room.RoomId?.ToString() ?? "?"} icon={room.IconToken ?? "<none>"}"
                    : string.Empty;

                SF.Chat.Add($"room[{room.RoomIndex}] type={room.Type} name={room.Name} color=0x{room.ColorArgb:X8}{dynamicInfo} {flags}");
            }
        });

        SFLog.Info("Program.Main entering module container run loop");
        await container.Run();
    }
}
