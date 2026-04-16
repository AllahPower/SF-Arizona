using System.Reflection;
using System.Runtime.Loader;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SFSharp;

internal static class PluginSharedAssemblyPolicy
{
    private static readonly Dictionary<string, Assembly> Shared = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SF.Abstractions"] = typeof(ISFModule).Assembly,
        ["Microsoft.Extensions.Logging.Abstractions"] = typeof(ILogger).Assembly,
        ["System.Text.Json"] = typeof(JsonSerializer).Assembly,
    };

    public static bool IsShared(string assemblyName) => Shared.ContainsKey(assemblyName);

    public static IReadOnlyCollection<string> Names => Shared.Keys;

    public static bool TryResolveLoadedAssembly(string assemblyName, out Assembly? assembly)
    {
        if (Shared.TryGetValue(assemblyName, out assembly))
        {
            return true;
        }

        assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(candidate =>
                !candidate.IsDynamic &&
                string.Equals(candidate.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase));

        return assembly is not null;
    }

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
