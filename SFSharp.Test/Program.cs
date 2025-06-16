using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SFSharp;

public static class Program
{
    [UnmanagedCallersOnly(EntryPoint = "SFSharpMain", CallConvs = [typeof(CallConvStdcall)])]
    public static int SFSharpMain(nint exports) => SFCore.Init(exports, Main);

    public static async void Main()
    {
        //await Task.Delay(10000);
        HookHelper.InstallSimpleHook();
        
        var container = new SFSharpModuleContainer();
        container.RegisterModule<BrightBinder>();
        container.RegisterModule<LicenseShooter>();
        container.RegisterModule<NodShaker>();
        container.RegisterModule<CountryCapitalHelper>(false);

        SF.RegisterChatCommand("sfd", x => SFDebug.ShowDialog());

        await container.RunAllAsync(CancellationToken.None);
    }
}