using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace SFSharp.Runtime.Modules;

/// <summary>
/// Collectible <see cref="AssemblyLoadContext"/> that isolates a single plugin's assembly graph
/// while re-using the host's <see cref="AssemblyLoadContext.Default"/> for a fixed set of shared
/// contracts. Without the shared routing, <c>typeof(ISFModule)</c> from the plugin would not equal
/// the one from the host and registration would fail with an obscure cast error.
/// </summary>
[RequiresDynamicCode("Plugin loading is unavailable under NativeAOT.")]
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;
    private readonly string _pluginId;
    private readonly Lock _sync = new();
    private readonly HashSet<string> _unresolvedManagedDependencies = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _unresolvedNativeDependencies = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Assembly> _sharedResolutionCache = new(StringComparer.OrdinalIgnoreCase);

    public PluginLoadContext(string pluginId, string pluginAssemblyPath)
        : base(name: $"SFPlugin:{pluginId}", isCollectible: true)
    {
        _pluginId = pluginId;
        _resolver = new AssemblyDependencyResolver(pluginAssemblyPath);
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
        if (assemblyName.Name is string name && PluginSharedAssemblyPolicy.IsShared(name))
        {
            lock (_sync)
            {
                if (_sharedResolutionCache.TryGetValue(name, out Assembly? cached))
                {
                    return cached;
                }
            }

            if (PluginSharedAssemblyPolicy.TryResolveLoadedAssembly(name, out Assembly? hostAsm) && hostAsm is not null)
            {
                lock (_sync)
                {
                    _sharedResolutionCache[name] = hostAsm;
                }

                SFLog.Debug($"PluginLoadContext[{_pluginId}] share '{name}' via host assembly {PluginSharedAssemblyPolicy.Describe(hostAsm)}");
                return hostAsm;
            }

            SFLog.Warn($"PluginLoadContext[{_pluginId}] shared '{name}' not found among loaded host assemblies — plugin may fail");
            return null;
        }

        string? path = _resolver.ResolveAssemblyToPath(assemblyName);
        if (path is null)
        {
            if (!string.IsNullOrWhiteSpace(assemblyName.Name) &&
                !assemblyName.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase) &&
                !IsBclAssembly(assemblyName.Name))
            {
                lock (_sync)
                {
                    _unresolvedManagedDependencies.Add(assemblyName.Name);
                }

                SFLog.Debug($"PluginLoadContext[{_pluginId}] unresolved managed '{assemblyName.Name}'");
            }

            return null;
        }

        SFLog.Debug($"PluginLoadContext[{_pluginId}] load '{assemblyName.Name}' from {path}");
        return LoadFromAssemblyPath(path);
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
