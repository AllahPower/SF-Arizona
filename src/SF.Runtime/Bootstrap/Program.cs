namespace SFSharp.Runtime.Bootstrap;

public static class Program
{
    internal static async void Main(SFRuntime runtime)
    {
        try
        {
            SFLog.Info("Program.Main started");

            SFHostManifest.Instance.Load();

            SFHost host = runtime.Host;
            host.ChatImpl.Add($"{{00FF00}}{RuntimeBuildInfo.ProductName} {{FFFFFF}}v{RuntimeBuildInfo.DisplayVersion}");
            host.ChatImpl.Add("{95FF4F}github.com/AllahPower/SF-Arizona | by AllahPower");

            var container = new SFModuleContainer(host, runtime.MainThread);
            container.RegisterModule(() => new RpcDebugger(host));
            container.RegisterModule(() => new DebugModule(host, runtime.MainThread, runtime.Exceptions));

            PluginLoader pluginLoader = new(container);
            container.PluginLoader = pluginLoader;
            int loadedPluginCount = pluginLoader.DiscoverAndLoadAll();
            SFLog.Info($"Program.Main plugin discovery complete, loaded {loadedPluginCount} plugin(s)");

            SFHostManifest.Instance.FlushSync();

            using IDisposable runtimeInfoCommand = RuntimeInfoCommand.Register(runtime, pluginLoader);

            SFLog.Info("Program.Main entering module container run loop");
            await container.Run();
        }
        catch (Exception ex)
        {
            runtime.Exceptions.Report(ex);
        }
    }
}
