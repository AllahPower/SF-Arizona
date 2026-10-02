namespace SFSharp.Runtime.Bootstrap;

public static class Program
{
    /// <summary>
    /// First runtime tick, while GTA is still loading: registers modules and loads plugins so their early modules
    /// see every load stage. Module factories resolve the host lazily because it exists only after CNetGame.
    /// </summary>
    internal static void LoadModules(SFRuntime runtime)
    {
        SFLog.Info("Program.LoadModules started");

        SFHostManifest.Instance.Load();

        SFModuleContainer container = runtime.Modules;
        container.RegisterModule(() => new RpcDebugger(runtime.Host));
        container.RegisterModule(() => new DebugModule(runtime.Host, runtime.MainThread, runtime.Exceptions));

        int loadedPluginCount = runtime.Plugins.DiscoverAndLoadAll();
        SFLog.Info($"Program.LoadModules plugin discovery complete, loaded {loadedPluginCount} plugin(s)");

        SFHostManifest.Instance.FlushSync();
    }

    internal static async void Main(SFRuntime runtime)
    {
        try
        {
            SFLog.Info("Program.Main started");

            SFHost host = runtime.Host;
            host.ChatImpl.Add($"{{00FF00}}{RuntimeBuildInfo.ProductName} {{FFFFFF}}v{RuntimeBuildInfo.DisplayVersion}");
            host.ChatImpl.Add("{95FF4F}github.com/AllahPower/SF-Arizona | by AllahPower");

            using IDisposable runtimeInfoCommand = RuntimeInfoCommand.Register(runtime, runtime.Plugins);

            SFLog.Info("Program.Main entering module container run loop");
            await runtime.Modules.Run();
        }
        catch (Exception ex)
        {
            runtime.Exceptions.Report(ex);
        }
    }
}
