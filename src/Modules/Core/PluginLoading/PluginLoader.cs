using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace SFSharp;

/// <summary>
/// Discovers, loads, unloads and hot-reloads third-party plugins from
/// <c>&lt;GameDir&gt;\SF\modules\*\module.json</c>. Each plugin is isolated inside its own
/// collectible <see cref="PluginLoadContext"/> and registers every type decorated with
/// <see cref="SFModuleAttribute"/> with the passed <see cref="SFModuleContainer"/>.
/// </summary>
/// <remarks>
/// Dynamic assembly loading is not supported under NativeAOT. On AOT builds
/// (<see cref="RuntimeFeature.IsDynamicCodeSupported"/> == <see langword="false"/>) the loader
/// emits a single warning and no-ops; the code path is fully exercised only after the host
/// migrates to CoreCLR per the plan in <c>src/dllmain.c</c>.
/// </remarks>
[RequiresDynamicCode("Third-party plugin loading requires dynamic code generation.")]
public sealed class PluginLoader
{
    private const string ManifestFileName = "module.json";
    private const int UnloadGcAttempts = 10;

    private readonly SFModuleContainer _container;
    private readonly string _pluginsRoot;
    private readonly Version _hostVersion;
    private readonly Dictionary<string, LoadedPlugin> _plugins = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();

    public PluginLoader(SFModuleContainer container)
        : this(container, Path.Combine(SFPaths.AssetsRoot, "modules"))
    {
    }

    public PluginLoader(SFModuleContainer container, string pluginsRoot)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginsRoot);
        _container = container;
        _pluginsRoot = pluginsRoot;
        _hostVersion = ResolveHostVersion();
    }

    /// <summary>Plugin ids currently loaded. Snapshot, safe to iterate after return.</summary>
    public IReadOnlyCollection<string> LoadedPluginIds
    {
        get
        {
            lock (_sync)
            {
                return _plugins.Keys.ToArray();
            }
        }
    }

    /// <summary>
    /// Enumerates <c>&lt;pluginsRoot&gt;/*/module.json</c> and loads every well-formed plugin.
    /// Errors are logged per-plugin and do not abort the scan.
    /// </summary>
    /// <returns>Number of successfully loaded plugins.</returns>
    public int DiscoverAndLoadAll()
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            SFLog.Warn($"PluginLoader: dynamic code is not supported (NativeAOT build). Plugin directory '{_pluginsRoot}' will not be scanned.");
            return 0;
        }

        if (!Directory.Exists(_pluginsRoot))
        {
            SFLog.Info($"PluginLoader: plugins root '{_pluginsRoot}' does not exist, nothing to load.");
            return 0;
        }

        SFLog.Info($"PluginLoader: scanning '{_pluginsRoot}' for manifests");
        int loaded = 0;
        foreach (string pluginDir in Directory.EnumerateDirectories(_pluginsRoot))
        {
            string manifestPath = Path.Combine(pluginDir, ManifestFileName);
            if (!File.Exists(manifestPath))
            {
                SFLog.Info($"PluginLoader: skip '{pluginDir}', no {ManifestFileName}");
                continue;
            }

            try
            {
                if (TryLoadFromManifest(manifestPath))
                {
                    loaded++;
                }
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, $"PluginLoader: unhandled error while loading '{manifestPath}'");
            }
        }

        SFLog.Info($"PluginLoader: discovery done, loaded {loaded} plugin(s)");
        return loaded;
    }

    /// <summary>Loads a single plugin from its <c>module.json</c>. Thread-safe.</summary>
    public bool TryLoadFromManifest(string manifestPath, out string pluginId, out int registeredModuleCount)
    {
        pluginId = string.Empty;
        registeredModuleCount = 0;
        return TryLoadFromManifestCore(manifestPath, out pluginId, out registeredModuleCount);
    }

    public bool TryLoadFromManifest(string manifestPath) => TryLoadFromManifestCore(manifestPath, out _, out _);

    private bool TryLoadFromManifestCore(string manifestPath, out string resolvedPluginId, out int registeredCount)
    {
        resolvedPluginId = string.Empty;
        registeredCount = 0;

        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            SFLog.Warn("PluginLoader: dynamic code is not supported, load request ignored.");
            return false;
        }

        if (!File.Exists(manifestPath))
        {
            SFLog.Error($"PluginLoader: manifest not found at '{manifestPath}'");
            return false;
        }

        PluginManifest? manifest;
        try
        {
            using FileStream stream = File.OpenRead(manifestPath);
            manifest = JsonSerializer.Deserialize(stream, PluginManifestJsonContext.Default.PluginManifest);
        }
        catch (JsonException ex)
        {
            SFLog.Error(ex, $"PluginLoader: malformed manifest '{manifestPath}'");
            return false;
        }
        catch (IOException ex)
        {
            SFLog.Error(ex, $"PluginLoader: cannot read manifest '{manifestPath}'");
            return false;
        }

        if (manifest is null || string.IsNullOrWhiteSpace(manifest.Id) || string.IsNullOrWhiteSpace(manifest.Assembly))
        {
            SFLog.Error($"PluginLoader: manifest '{manifestPath}' is missing 'id' or 'assembly'");
            return false;
        }

        string pluginId = manifest.Id.Trim();

        if (!string.IsNullOrWhiteSpace(manifest.MinHostVersion))
        {
            if (!Version.TryParse(manifest.MinHostVersion, out Version? required))
            {
                SFLog.Warn($"PluginLoader[{pluginId}]: minHostVersion '{manifest.MinHostVersion}' is not a valid version, ignoring");
            }
            else if (required > _hostVersion)
            {
                SFLog.Error($"PluginLoader[{pluginId}]: requires host >= {required}, current is {_hostVersion}. Skipping.");
                return false;
            }
        }

        lock (_sync)
        {
            if (_plugins.ContainsKey(pluginId))
            {
                SFLog.Error($"PluginLoader[{pluginId}]: already loaded, refuse to re-register");
                return false;
            }
        }

        string pluginDir = Path.GetDirectoryName(manifestPath)!;
        string assemblyPath = Path.IsPathRooted(manifest.Assembly)
            ? manifest.Assembly
            : Path.GetFullPath(Path.Combine(pluginDir, manifest.Assembly));

        if (!File.Exists(assemblyPath))
        {
            SFLog.Error($"PluginLoader[{pluginId}]: assembly file not found at '{assemblyPath}'");
            return false;
        }

        PluginLoadContext context = new(pluginId, assemblyPath);
        Assembly assembly;
        try
        {
            assembly = context.LoadFromAssemblyPath(assemblyPath);
            SFLog.Info($"PluginLoader[{pluginId}]: loaded assembly {assembly.FullName}");
        }
        catch (Exception ex)
        {
            SFLog.Error(ex, $"PluginLoader[{pluginId}]: failed to load assembly '{assemblyPath}'");
            TryUnloadContext(context, pluginId);
            return false;
        }

        Type[] moduleTypes;
        try
        {
            moduleTypes = assembly.GetTypes()
                .Where(static type => !type.IsAbstract && !type.IsInterface && typeof(ISFModule).IsAssignableFrom(type) && type.GetCustomAttribute<SFModuleAttribute>() is not null)
                .ToArray();
        }
        catch (ReflectionTypeLoadException ex)
        {
            SFLog.Error(ex, $"PluginLoader[{pluginId}]: type enumeration failed");
            foreach (Exception? loader in ex.LoaderExceptions)
            {
                if (loader is not null)
                {
                    SFLog.Error(loader, $"PluginLoader[{pluginId}]: loader exception");
                }
            }

            TryUnloadContext(context, pluginId);
            return false;
        }

        if (moduleTypes.Length == 0)
        {
            SFLog.Warn($"PluginLoader[{pluginId}]: assembly has no types decorated with [SFModule], unloading.");
            TryUnloadContext(context, pluginId);
            return false;
        }

        List<string> registered = new(moduleTypes.Length);
        try
        {
            foreach (Type type in moduleTypes)
            {
                _container.RegisterModule(type, manifest.EnabledOnStart);
                ModuleDescriptor descriptor = ModuleDescriptor.FromType(type);
                registered.Add(descriptor.Id);
                SFLog.Info($"PluginLoader[{pluginId}]: registered module id={descriptor.Id} type={type.FullName}");
            }
        }
        catch (Exception ex)
        {
            SFLog.Error(ex, $"PluginLoader[{pluginId}]: registration failed, rolling back");
            foreach (string id in registered)
            {
                _container.TryUnregisterModule(id);
            }

            TryUnloadContext(context, pluginId);
            return false;
        }

        LoadedPlugin record = new()
        {
            PluginId = pluginId,
            ManifestPath = manifestPath,
            AssemblyPath = assemblyPath,
            LoadContext = context,
            Assembly = assembly,
            RegisteredModuleIds = registered,
            LoadContextRef = new WeakReference(context),
        };

        lock (_sync)
        {
            _plugins.Add(pluginId, record);
        }

        resolvedPluginId = pluginId;
        registeredCount = registered.Count;
        SFLog.Info($"PluginLoader[{pluginId}]: loaded {registered.Count} module(s)");
        return true;
    }

    /// <summary>Stops every module from the plugin, unregisters them, then unloads the ALC.</summary>
    public bool TryUnload(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        LoadedPlugin? plugin;
        lock (_sync)
        {
            if (!_plugins.TryGetValue(pluginId, out plugin))
            {
                SFLog.Warn($"PluginLoader[{pluginId}]: unload requested but plugin is not loaded");
                return false;
            }

            _plugins.Remove(pluginId);
        }

        SFLog.Info($"PluginLoader[{pluginId}]: unloading {plugin.RegisteredModuleIds.Count} module(s)");
        foreach (string moduleId in plugin.RegisteredModuleIds)
        {
            _container.RequestStopModule(moduleId, ModuleStopReason.PluginUnload);
        }

        _container.WaitForModulesStopped(plugin.RegisteredModuleIds, TimeSpan.FromSeconds(5));

        foreach (string moduleId in plugin.RegisteredModuleIds)
        {
            _container.TryUnregisterModule(moduleId);
        }

        TryUnloadContext(plugin.LoadContext, pluginId);

        for (int attempt = 0; attempt < UnloadGcAttempts && plugin.LoadContextRef.IsAlive; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        if (plugin.LoadContextRef.IsAlive)
        {
            SFLog.Warn($"PluginLoader[{pluginId}]: ALC still alive after {UnloadGcAttempts} GC cycles. Something holds a reference to plugin types.");
        }
        else
        {
            SFLog.Info($"PluginLoader[{pluginId}]: ALC fully collected");
        }

        return true;
    }

    /// <summary>Unload + re-scan the manifest. Convenience wrapper for <c>/sfs reload</c>.</summary>
    public bool TryReload(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        string? manifestPath;
        lock (_sync)
        {
            if (!_plugins.TryGetValue(pluginId, out LoadedPlugin? existing))
            {
                SFLog.Warn($"PluginLoader[{pluginId}]: reload requested but plugin is not loaded");
                return false;
            }

            manifestPath = existing.ManifestPath;
        }

        SFLog.Info($"PluginLoader[{pluginId}]: reload starting");
        if (!TryUnload(pluginId))
        {
            return false;
        }

        return TryLoadFromManifest(manifestPath);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TryUnloadContext(PluginLoadContext context, string pluginId)
    {
        try
        {
            context.Unload();
        }
        catch (InvalidOperationException ex)
        {
            SFLog.Error(ex, $"PluginLoader[{pluginId}]: ALC.Unload threw");
        }
    }

    private static Version ResolveHostVersion()
    {
        string? informational = typeof(PluginLoader).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            int plus = informational.IndexOf('+');
            string trimmed = plus < 0 ? informational : informational[..plus];
            if (Version.TryParse(trimmed, out Version? parsed))
            {
                return parsed;
            }
        }

        return typeof(PluginLoader).Assembly.GetName().Version ?? new Version(0, 0, 0, 0);
    }
}
