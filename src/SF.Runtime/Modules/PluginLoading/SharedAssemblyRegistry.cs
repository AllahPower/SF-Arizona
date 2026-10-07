using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;

namespace SFSharp.Runtime.Modules.PluginLoading;

/// <summary>
/// Assemblies that plugins export through <c>sharedAssemblies</c>. Each one is loaded from its plugin directory into the
/// Default context on first use and stays there for the process lifetime, so the exporting plugin and its dependents
/// see one type identity and the exporter can keep native hooks or static state across its own reloads.
/// </summary>
internal sealed class SharedAssemblyRegistry
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, Entry> _entries = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _hostDirectory;

    public SharedAssemblyRegistry()
        : this(Path.GetDirectoryName(typeof(ISFModule).Assembly.Location)!)
    {
    }

    public SharedAssemblyRegistry(string hostDirectory)
    {
        _hostDirectory = hostDirectory;
        // Exported assemblies may reference each other (SF.UI -> MinHook.NET); those binds happen in the Default context.
        AssemblyLoadContext.Default.Resolving += (_, name) => name.Name is string simpleName ? Load(simpleName) : null;
    }

    /// <summary>Records the exports of a plugin. Nothing is recorded when any of them conflicts.</summary>
    public bool TryRegister(string pluginId, IReadOnlyDictionary<string, string> paths, List<string> warnings, out string? error)
    {
        error = null;
        lock (_sync)
        {
            foreach ((string name, string path) in paths)
            {
                if (_entries.TryGetValue(name, out Entry? existing))
                {
                    if (!string.Equals(existing.Owner, pluginId, StringComparison.OrdinalIgnoreCase)
                        || !string.Equals(existing.Path, path, StringComparison.OrdinalIgnoreCase))
                    {
                        error = $"shared assembly '{name}' is already exported by plugin '{existing.Owner}' from '{existing.Path}'";
                        return false;
                    }

                    continue;
                }

                string hostCopy = Path.Combine(_hostDirectory, name + ".dll");
                if (File.Exists(hostCopy))
                {
                    error = $"'{hostCopy}' shadows the shared assembly '{name}' of this plugin. Delete it and restart the game";
                    return false;
                }

                Assembly? loaded = FindInDefault(name);
                if (loaded is not null && !string.Equals(loaded.Location, path, StringComparison.OrdinalIgnoreCase))
                {
                    error = $"'{name}' is already loaded from '{loaded.Location}'. Remove that copy and restart the game";
                    return false;
                }
            }

            foreach ((string name, string path) in paths)
            {
                if (_entries.TryGetValue(name, out Entry? existing))
                {
                    if (existing.Assembly is Assembly assembly && ReadMvid(path) is Guid onDisk && onDisk != assembly.ManifestModule.ModuleVersionId)
                    {
                        warnings.Add($"shared assembly '{name}' changed on disk; the loaded copy is used until the game restarts");
                    }

                    continue;
                }

                _entries.Add(name, new Entry(pluginId, path) { Assembly = FindInDefault(name) });
            }
        }

        return true;
    }

    /// <summary>Resolves <paramref name="name"/> when one of <paramref name="owners"/> exports it.</summary>
    public bool TryResolve(string name, IReadOnlySet<string> owners, out Assembly? assembly)
    {
        lock (_sync)
        {
            if (!_entries.TryGetValue(name, out Entry? entry) || !owners.Contains(entry.Owner))
            {
                assembly = null;
                return false;
            }
        }

        assembly = Load(name);
        return assembly is not null;
    }

    /// <summary>Plugin that exports <paramref name="name"/>, if any. Used to explain missing dependencies.</summary>
    public string? FindOwner(string name)
    {
        lock (_sync)
        {
            return _entries.TryGetValue(name, out Entry? entry) ? entry.Owner : null;
        }
    }

    private Assembly? Load(string name)
    {
        Entry? entry;
        lock (_sync)
        {
            if (!_entries.TryGetValue(name, out entry))
            {
                return null;
            }

            if (entry.Assembly is not null)
            {
                return entry.Assembly;
            }
        }

        // Loaded outside the lock: the binder may raise Resolving on another thread that needs this registry.
        // Loading the same path twice into the Default context returns the same assembly.
        Assembly assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(entry.Path);
        lock (_sync)
        {
            if (entry.Assembly is null)
            {
                entry.Assembly = assembly;
                SFLog.Info($"SharedAssemblyRegistry: loaded '{name}' exported by '{entry.Owner}' from {entry.Path}");
            }

            return entry.Assembly;
        }
    }

    private static Assembly? FindInDefault(string name)
        => AssemblyLoadContext.Default.Assemblies.FirstOrDefault(assembly =>
            string.Equals(assembly.GetName().Name, name, StringComparison.OrdinalIgnoreCase));

    private static Guid? ReadMvid(string path)
    {
        try
        {
            using FileStream stream = File.OpenRead(path);
            using PEReader peReader = new(stream);
            MetadataReader reader = peReader.GetMetadataReader();
            return reader.GetGuid(reader.GetModuleDefinition().Mvid);
        }
        catch (Exception ex) when (ex is IOException or BadImageFormatException or UnauthorizedAccessException or InvalidOperationException)
        {
            return null;
        }
    }

    private sealed class Entry(string owner, string path)
    {
        public string Owner { get; } = owner;
        public string Path { get; } = path;
        public Assembly? Assembly { get; set; }
    }
}
