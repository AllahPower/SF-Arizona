namespace SFSharp.Runtime.Bootstrap;

public static class Program
{
    public static async void Main()
    {
        try
        {
            SFLog.Info("Program.Main started");

            SFHostManifest.Instance.Load();

            SF.Chat.Add($"{{00FF00}}{RuntimeBuildInfo.ProductName} {{FFFFFF}}v{RuntimeBuildInfo.DisplayVersion}");
            SF.Chat.Add("{95FF4F}github.com/AllahPower/SF-Arizona | by AllahPower");

            var container = new SFModuleContainer();
            container.RegisterModule<RpcDebugger>();
            container.RegisterModule<DebugModule>();

#pragma warning disable IL3050
            PluginLoader pluginLoader = new(container);
            container.PluginLoader = pluginLoader;
            int loadedPluginCount = pluginLoader.DiscoverAndLoadAll();
            SFLog.Info($"Program.Main plugin discovery complete, loaded {loadedPluginCount} plugin(s)");
#pragma warning restore IL3050

            SFHostManifest.Instance.FlushSync();

            using IDisposable runtimeInfoCommand = RuntimeInfoCommand.Register(pluginLoader);

            SFLog.Info("Program.Main entering module container run loop");
            await container.Run();
        }
        catch (Exception ex)
        {
            SFBootstrap.ProcessException(ex);
        }
    }
}
