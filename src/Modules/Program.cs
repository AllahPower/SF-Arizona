using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SFSharp;

public static class Program
{
    public static async void Main()
    {
        SFLog.Info("Program.Main started");

        var container = new SFModuleContainer();
        container.RegisterModule<DialogScraper>();
        container.RegisterModule<BrightBinder>();
        container.RegisterModule<LicenseShooter>();
        container.RegisterModule<NodShaker>();
        container.RegisterModule<ChatViolationMonitor>();

        using var debugCommand = SF.Chat.RegisterChatCommand("sfd", _ =>
        {
            SFLog.Info("Debug command /sfd executed");
            SF.Chat.Add("Hello from SF-Arizona debug command.");
        });

        SFLog.Info("Program.Main entering module container run loop");
        await container.Run();
    }
}
