using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace SFSharp;

/// <summary>
/// Collectible <see cref="AssemblyLoadContext"/> that isolates a single plugin's assembly graph
/// while re-using the host's <see cref="AssemblyLoadContext.Default"/> for a fixed set of shared
/// contracts. Without the shared routing, <c>typeof(ISFModule)</c> from the plugin would not equal
/// the one from the host and registration would fail with an obscure cast error.
/// </summary>
[RequiresDynamicCode("Plugin loading is unavailable under NativeAOT.")]
internal sealed class PluginLoadContext : AssemblyLoadContext
{
    private static readonly HashSet<string> SharedAssemblies = new(StringComparer.OrdinalIgnoreCase)
    {
        "SF.Abstractions",
        "Microsoft.Extensions.Logging.Abstractions",
        "System.Text.Json",
    };

    private readonly AssemblyDependencyResolver _resolver;
    private readonly string _pluginId;

    public PluginLoadContext(string pluginId, string pluginAssemblyPath)
        : base(name: $"SFPlugin:{pluginId}", isCollectible: true)
    {
        _pluginId = pluginId;
        _resolver = new AssemblyDependencyResolver(pluginAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is string name && SharedAssemblies.Contains(name))
        {
            SFLog.Info($"PluginLoadContext[{_pluginId}] share '{name}' via Default ALC");
            return null;
        }

        string? path = _resolver.ResolveAssemblyToPath(assemblyName);
        if (path is null)
        {
            return null;
        }

        SFLog.Info($"PluginLoadContext[{_pluginId}] load '{assemblyName.Name}' from {path}");
        return LoadFromAssemblyPath(path);
    }

    protected override nint LoadUnmanagedDll(string unmanagedDllName)
    {
        string? path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        if (path is null)
        {
            return nint.Zero;
        }

        SFLog.Info($"PluginLoadContext[{_pluginId}] load native '{unmanagedDllName}' from {path}");
        return LoadUnmanagedDllFromPath(path);
    }
}
