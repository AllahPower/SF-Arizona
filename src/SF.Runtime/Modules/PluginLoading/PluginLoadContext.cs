using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace SFSharp.Runtime.Modules.PluginLoading;

/// <summary>
/// Collectible <see cref="AssemblyLoadContext"/> that isolates a single plugin's assembly graph.
/// Resolution order: host contracts, shared assemblies exported by the plugin or its direct plugin dependencies,
/// the plugin's own directory, then libraries installed beside the host. Host contracts must come from the
/// Default context, otherwise <c>typeof(ISFModule)</c> from the plugin would not equal the host's and
/// registration would fail with an obscure cast error.
/// </summary>
[RequiresDynamicCode("Plugin loading is unavailable under NativeAOT.")]
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly string _pluginId;
    private readonly SharedAssemblyRegistry _sharedAssemblies;
    private readonly IReadOnlySet<string> _sharedAssemblyOwners;
    private readonly Lock _sync = new();
    private readonly HashSet<string> _unresolvedManagedDependencies = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _unresolvedNativeDependencies = new(StringComparer.OrdinalIgnoreCase);

    public PluginLoadContext(ResolvedPluginManifest manifest, SharedAssemblyRegistry sharedAssemblies)
        : base(name: $"SFPlugin:{manifest.PluginId}", isCollectible: true)
    {
        _pluginId = manifest.PluginId;
        _resolver = new AssemblyDependencyResolver(manifest.AssemblyPath);
        _sharedAssemblies = sharedAssemblies;
        _sharedAssemblyOwners = manifest.SharedAssemblyOwners();
    }

    public string[] SnapshotUnresolvedManagedDependencies()
    {
        lock (_sync)
        {
            return [.. _unresolvedManagedDependencies];
        }
    }

    public string[] SnapshotUnresolvedNativeDependencies()
    {
        lock (_sync)
        {
            return [.. _unresolvedNativeDependencies];
        }
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        string? name = assemblyName.Name;
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (PluginSharedAssemblyPolicy.IsShared(name))
        {
            if (PluginSharedAssemblyPolicy.TryResolveLoadedAssembly(name, out Assembly? contract) && contract is not null)
            {
                SFLog.Debug($"PluginLoadContext[{_pluginId}] share '{name}' via host assembly {PluginSharedAssemblyPolicy.Describe(contract)}");
                return contract;
            }

            SFLog.Warn($"PluginLoadContext[{_pluginId}] host contract '{name}' is not loaded, plugin may fail");
            return null;
        }

        if (_sharedAssemblies.TryResolve(name, _sharedAssemblyOwners, out Assembly? exported) && exported is not null)
        {
            SFLog.Debug($"PluginLoadContext[{_pluginId}] share '{name}' via export of '{_sharedAssemblies.FindOwner(name)}' {PluginSharedAssemblyPolicy.Describe(exported)}");
            return exported;
        }

        string? path = _resolver.ResolveAssemblyToPath(assemblyName);
        if (path is not null)
        {
            SFLog.Debug($"PluginLoadContext[{_pluginId}] load '{name}' from {path}");
            return LoadFromAssemblyPath(path);
        }

        // Returning null would fall back to the Default context, which binds to the export by name once any plugin loaded it.
        // Failing here keeps the result independent of load order.
        if (_sharedAssemblies.FindOwner(name) is string owner)
        {
            RecordUnresolved(name);
            string message = $"'{name}' is exported by plugin '{owner}'. Declare '{owner}' in the dependencies of plugin '{_pluginId}'.";
            SFLog.Warn($"PluginLoadContext[{_pluginId}] {message}");
            throw new FileNotFoundException(message, name);
        }

        if (PluginSharedAssemblyPolicy.IsHostLibrary(assemblyName))
        {
            Assembly hostLibrary = PluginSharedAssemblyPolicy.ResolveHostLibrary(assemblyName);
            SFLog.Debug($"PluginLoadContext[{_pluginId}] share '{name}' via host library {PluginSharedAssemblyPolicy.Describe(hostLibrary)}");
            return hostLibrary;
        }

        if (!name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase) && !IsBclAssembly(name))
        {
            RecordUnresolved(name);
            SFLog.Debug($"PluginLoadContext[{_pluginId}] unresolved managed '{name}'");
        }

        return null;
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        string? path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (path is null)
        {
            lock (_sync)
            {
                _unresolvedNativeDependencies.Add(unmanagedDllName);
            }

            SFLog.Debug($"PluginLoadContext[{_pluginId}] unresolved native '{unmanagedDllName}'");
            return nint.Zero;
        }

        SFLog.Debug($"PluginLoadContext[{_pluginId}] load native '{unmanagedDllName}' from {path}");
        return LoadUnmanagedDllFromPath(path);
    }

    private void RecordUnresolved(string name)
    {
        lock (_sync)
        {
            _unresolvedManagedDependencies.Add(name);
        }
    }

    private static bool IsBclAssembly(string name)
    {
        return name.StartsWith("System.", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)
            || name.Equals("System", StringComparison.OrdinalIgnoreCase)
            || name.Equals("mscorlib", StringComparison.OrdinalIgnoreCase)
            || name.Equals("netstandard", StringComparison.OrdinalIgnoreCase)
            || name.Equals("WindowsBase", StringComparison.OrdinalIgnoreCase);
    }
}
