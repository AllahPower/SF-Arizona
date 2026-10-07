using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;

namespace SFSharp.Runtime.Modules.PluginLoading;

internal static class PluginSharedAssemblyPolicy
{
    private static readonly Dictionary<string, Assembly> Shared = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SF.Abstractions"] = typeof(ISFModule).Assembly,
        ["SF.Protocol"] = typeof(SampRpc).Assembly,
        ["Microsoft.Extensions.Logging.Abstractions"] = typeof(ILogger).Assembly,
        ["System.Text.Json"] = typeof(JsonSerializer).Assembly,
    };

    public static bool IsShared(string assemblyName) => Shared.ContainsKey(assemblyName);

    // Fallback for libraries the plugin does not ship itself: a copy in the plugin directory always wins.
    public static bool IsHostLibrary(AssemblyName requested)
    {
        string? name = requested.Name;
        return !string.IsNullOrWhiteSpace(name)
            && string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal)
            && File.Exists(Path.Combine(Path.GetDirectoryName(typeof(ISFModule).Assembly.Location)!, name + ".dll"));
    }

    public static Assembly ResolveHostLibrary(AssemblyName requested)
    {
        if (!IsHostLibrary(requested)) throw new FileNotFoundException($"Shared host library '{requested.Name}' was not installed.");
        string path = Path.Combine(Path.GetDirectoryName(typeof(ISFModule).Assembly.Location)!, requested.Name + ".dll");
        Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
        if (!string.Equals(assembly.GetName().Name, requested.Name, StringComparison.OrdinalIgnoreCase))
            throw new FileLoadException($"Shared host library '{path}' has a different assembly identity.");
        if (!string.Equals(Path.GetFullPath(assembly.Location), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase))
            throw new FileLoadException($"Shared host library '{requested.Name}' was already loaded from another location.");
        if (requested.Version is Version version && assembly.GetName().Version < version)
            throw new FileLoadException($"Shared host library '{requested.Name}' is older than the plugin requires ({version}).");
        return assembly;
    }

    public static IReadOnlyCollection<string> Names => Shared.Keys;

    // Only the fixed contract set: scanning the AppDomain could bind the Default context to a collectible plugin assembly.
    public static bool TryResolveLoadedAssembly(string assemblyName, out Assembly? assembly)
        => Shared.TryGetValue(assemblyName, out assembly);

    public static string Describe(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        AssemblyLoadContext? alc = AssemblyLoadContext.GetLoadContext(assembly);
        string alcName = alc?.Name ?? "<null>";
        string location = string.IsNullOrEmpty(assembly.Location) ? "<dynamic>" : assembly.Location;
        Guid mvid = assembly.ManifestModule.ModuleVersionId;
        return $"{assembly.FullName} | alc={alcName} collectible={alc?.IsCollectible ?? false} | mvid={mvid} | loc={location}";
    }
}
