using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SFSharp;

public static class Program
{
    public static async void Main()
    {
        var container = new SFModuleContainer();
        container.RegisterModule<DialogScraper>();
        container.RegisterModule<BrightBinder>();
        container.RegisterModule<LicenseShooter>(false);
        container.RegisterModule<NodShaker>();

        await container.Run();
    }
}