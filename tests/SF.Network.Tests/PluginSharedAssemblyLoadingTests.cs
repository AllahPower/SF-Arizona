using System.Reflection;
using System.Runtime.Loader;
using SFSharp.Runtime.Bootstrap;
using SFSharp.Runtime.Modules.PluginLoading;

namespace SF.Network.Tests;

/// <summary>
/// Loads the fixture plugins through <see cref="PluginLoader"/>. The exported fixture assemblies stay in the Default
/// context for the rest of the test run, so every test uses the same provider directory.
/// </summary>
public sealed class PluginSharedAssemblyLoadingTests
{
    private const string ProviderId = "fixture.provider";
    private const string ConsumerId = "fixture.consumer";
    private const string SharedLibName = "SF.Fixture.SharedLib";

    private static readonly Lazy<string> PluginsRoot = new(CreatePluginsRoot);
    private static readonly Lock Sequential = new();

    [Fact]
    public void DependentPluginBindsToTheExportedCopyAndBothUnload()
    {
        lock (Sequential)
        {
            RunOnRuntime((runtime, loader) =>
            {
                int before = SharedCounterValue();

                Assert.True(loader.LoadFromManifest(ManifestPath("provider")).Success);
                PluginLoadResult consumer = loader.LoadFromManifest(ManifestPath("consumer"));
                Assert.True(consumer.Success, consumer.Message);

                Assembly shared = DefaultAssembly(SharedLibName)!;
                Assert.StartsWith(Path.Combine(PluginsRoot.Value, "provider"), shared.Location, StringComparison.OrdinalIgnoreCase);
                Assert.StartsWith(Path.Combine(PluginsRoot.Value, "provider"), DefaultAssembly("SF.Fixture.SharedLibDep")!.Location, StringComparison.OrdinalIgnoreCase);
                Assert.Equal(before + 2, SharedCounterValue());

                PluginUnloadResult consumerUnload = loader.Unload(ConsumerId);
                Assert.True(consumerUnload.Success, consumerUnload.Message);
                PluginUnloadResult providerUnload = loader.Unload(ProviderId);
                Assert.True(providerUnload.Success, providerUnload.Message);

                PluginLoadResult reload = loader.LoadFromManifest(ManifestPath("provider"));
                Assert.True(reload.Success, reload.Message);
                Assert.Same(shared, DefaultAssembly(SharedLibName));
                Assert.Equal(before + 3, SharedCounterValue());
                Assert.True(loader.Unload(ProviderId).Success);
            });
        }
    }

    [Fact]
    public void ExportIsRejectedForPluginsThatDoNotDependOnTheExporter()
    {
        lock (Sequential)
        {
            RunOnRuntime((runtime, loader) =>
            {
                Assert.True(loader.LoadFromManifest(ManifestPath("provider")).Success);

                PluginLoadResult result = loader.LoadFromManifest(ManifestPath("consumer-undeclared"));

                Assert.False(result.Success);
                Assert.Equal(PluginLoadFailureReason.EarlyModuleFailed, result.FailureReason);
                Assert.Contains($"Declare '{ProviderId}'", result.Message);
                Assert.True(loader.Unload(ProviderId).Success);
            });
        }
    }

    [Fact]
    public void ProviderCannotUnloadWhileItsDependentIsLoaded()
    {
        lock (Sequential)
        {
            RunOnRuntime((runtime, loader) =>
            {
                Assert.True(loader.LoadFromManifest(ManifestPath("provider")).Success);
                Assert.True(loader.LoadFromManifest(ManifestPath("consumer")).Success);

                PluginUnloadResult blocked = loader.Unload(ProviderId);

                Assert.Equal(PluginUnloadFailureReason.DependentPluginsLoaded, blocked.FailureReason);
                Assert.True(loader.Unload(ConsumerId).Success);
                Assert.True(loader.Unload(ProviderId).Success);
            });
        }
    }

    [Fact]
    public void ExportReferencingAnUnsharedPluginAssemblyIsRejected()
    {
        PluginManifestResolutionResult result = new PluginManifestResolver().Resolve(ManifestPath("provider-partial"));

        Assert.False(result.Success);
        Assert.Equal(PluginManifestResolutionFailureReason.InvalidSharedAssembly, result.FailureReason);
        Assert.Contains("references 'SF.Fixture.SharedLibDep'", result.Message);
    }

    [Fact]
    public void RegistryRejectsAnAssemblyExportedByAnotherPlugin()
    {
        string root = Directory.CreateTempSubdirectory("sf-registry-").FullName;
        try
        {
            SharedAssemblyRegistry registry = new(root);
            Dictionary<string, string> first = new() { ["Fixture.Conflict"] = Path.Combine(root, "a", "Fixture.Conflict.dll") };
            Dictionary<string, string> second = new() { ["Fixture.Conflict"] = Path.Combine(root, "b", "Fixture.Conflict.dll") };

            Assert.True(registry.TryRegister("a", first, [], out _));
            Assert.True(registry.TryRegister("a", first, [], out _));
            Assert.False(registry.TryRegister("b", second, [], out string? error));
            Assert.Contains("already exported by plugin 'a'", error);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void RegistryRejectsAnExportShadowedByAHostCopy()
    {
        string root = Directory.CreateTempSubdirectory("sf-registry-").FullName;
        try
        {
            File.WriteAllBytes(Path.Combine(root, "Fixture.Shadowed.dll"), []);
            SharedAssemblyRegistry registry = new(root);

            Assert.False(registry.TryRegister("ui", new Dictionary<string, string> { ["Fixture.Shadowed"] = Path.Combine(root, "ui", "Fixture.Shadowed.dll") }, [], out string? error));
            Assert.Contains("Delete it and restart the game", error);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static void RunOnRuntime(Action<SFRuntime, PluginLoader> test)
    {
        SFRuntime runtime = new();
        PluginLoader loader = new(runtime.Modules, runtime.Loading, new SharedAssemblyRegistry(Path.GetDirectoryName(typeof(PluginLoader).Assembly.Location)!), PluginsRoot.Value);
        SynchronizationContext? previous = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(runtime.Context);
        try
        {
            test(runtime, loader);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(previous);
        }
    }

    private static string ManifestPath(string directory) => Path.Combine(PluginsRoot.Value, directory, PluginManifest.FileName);

    private static Assembly? DefaultAssembly(string name)
        => AssemblyLoadContext.Default.Assemblies.FirstOrDefault(assembly => assembly.GetName().Name == name);

    private static int SharedCounterValue()
        => DefaultAssembly(SharedLibName) is Assembly shared
            ? (int)shared.GetType("SF.Fixture.SharedLib.SharedCounter", throwOnError: true)!.GetProperty("Value")!.GetValue(null)!
            : 0;

    private static string CreatePluginsRoot()
    {
        // Fixtures build to artifacts/bin/<project>/<configuration>/net10.0, next to this test assembly.
        DirectoryInfo output = new(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar));
        string configuration = output.Parent!.Name;
        string bin = output.Parent!.Parent!.Parent!.FullName;
        string root = Path.Combine(output.FullName, "plugin-fixtures");
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }

        string FixtureOutput(string project) => Path.Combine(bin, project, configuration, "net10.0");

        CopyPlugin(FixtureOutput("SF.Fixture.Provider"), Path.Combine(root, "provider"), ["SF.Fixture.Provider", SharedLibName, "SF.Fixture.SharedLibDep"]);
        File.WriteAllText(Path.Combine(root, "provider", PluginManifest.FileName), $$"""
            { "id": "{{ProviderId}}", "version": "1.0.0", "assembly": "SF.Fixture.Provider.dll",
              "sharedAssemblies": ["{{SharedLibName}}", "SF.Fixture.SharedLibDep"] }
            """);

        CopyPlugin(FixtureOutput("SF.Fixture.Provider"), Path.Combine(root, "provider-partial"), ["SF.Fixture.Provider", SharedLibName, "SF.Fixture.SharedLibDep"]);
        File.WriteAllText(Path.Combine(root, "provider-partial", PluginManifest.FileName), $$"""
            { "id": "fixture.provider-partial", "version": "1.0.0", "assembly": "SF.Fixture.Provider.dll",
              "sharedAssemblies": ["{{SharedLibName}}"] }
            """);

        CopyPlugin(FixtureOutput("SF.Fixture.Consumer"), Path.Combine(root, "consumer"), ["SF.Fixture.Consumer"]);
        File.WriteAllText(Path.Combine(root, "consumer", PluginManifest.FileName), $$"""
            { "id": "{{ConsumerId}}", "version": "1.0.0", "assembly": "SF.Fixture.Consumer.dll",
              "dependencies": { "{{ProviderId}}": { "min": "1.0.0" } } }
            """);

        CopyPlugin(FixtureOutput("SF.Fixture.Consumer"), Path.Combine(root, "consumer-undeclared"), ["SF.Fixture.Consumer"]);
        File.WriteAllText(Path.Combine(root, "consumer-undeclared", PluginManifest.FileName), """
            { "id": "fixture.consumer-undeclared", "version": "1.0.0", "assembly": "SF.Fixture.Consumer.dll" }
            """);

        return root;
    }

    private static void CopyPlugin(string source, string destination, string[] assemblies)
    {
        Directory.CreateDirectory(destination);
        foreach (string assembly in assemblies)
        {
            File.Copy(Path.Combine(source, assembly + ".dll"), Path.Combine(destination, assembly + ".dll"));
        }

        File.Copy(Path.Combine(source, assemblies[0] + ".deps.json"), Path.Combine(destination, assemblies[0] + ".deps.json"));
    }
}
