using SFSharp.Runtime.Bootstrap;
using SFSharp.Runtime.Modules.PluginLoading;

namespace SF.Network.Tests;

public sealed class PluginDiscoveryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DiscoveryCreatesMissingRootAndPreservesExistingFiles(bool rootExists)
    {
        DirectoryInfo temporary = Directory.CreateTempSubdirectory("sf-plugin-discovery-");
        string root = Path.Combine(temporary.FullName, "SF", "modules");
        string marker = Path.Combine(root, "keep.txt");
        SynchronizationContext? previous = SynchronizationContext.Current;
        try
        {
            if (rootExists)
            {
                Directory.CreateDirectory(root);
                File.WriteAllText(marker, "keep");
            }

            SFRuntime runtime = new();
            PluginLoader loader = new(runtime.Modules, runtime.Loading,
                new SharedAssemblyRegistry(Path.GetDirectoryName(typeof(PluginLoader).Assembly.Location)!), root);
            SynchronizationContext.SetSynchronizationContext(runtime.Context);

            Assert.Equal(0, loader.DiscoverAndLoadAll());
            Assert.True(Directory.Exists(root));
            Assert.Equal(0, loader.DiscoverAndLoadAll());
            Assert.Empty(loader.LoadedPluginIds);
            if (rootExists)
                Assert.Equal("keep", File.ReadAllText(marker));
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
            temporary.Delete(recursive: true);
        }
    }
}
