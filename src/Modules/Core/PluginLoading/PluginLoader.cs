using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SFSharp.Runtime.Modules;

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
    private static readonly TimeSpan UnloadStopTimeout = TimeSpan.FromSeconds(5);

    private readonly SFModuleContainer _container;
    private readonly string _pluginsRoot;
    private readonly Version _hostVersion;
    private readonly PluginManifestResolver _manifestResolver = new();
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

    public IReadOnlyCollection<PluginRuntimeSnapshot> LoadedPlugins
    {
        get
        {
            lock (_sync)
            {
                return _plugins.Values.Select(static plugin => plugin.CreateSnapshot()).ToArray();
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
        return ExecuteOnContainerThread(DiscoverAndLoadAllCore);
    }

    private int DiscoverAndLoadAllCore()
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
                if (LoadFromManifestCore(manifestPath).Success)
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

    /// <summary>
    /// Loads a single plugin from its <c>module.json</c>. Safe to call from any thread; the
    /// actual mutation is serialized onto the container/main thread.
    /// </summary>
    public bool TryLoadFromManifest(string manifestPath, out string pluginId, out int registeredModuleCount)
    {
        PluginLoadResult result = LoadFromManifest(manifestPath);
        pluginId = result.PluginId ?? string.Empty;
        registeredModuleCount = result.RegisteredModuleCount;
        return result.Success;
    }

    public bool TryLoadFromManifest(string manifestPath) => LoadFromManifest(manifestPath).Success;

    public PluginLoadResult LoadFromManifest(string manifestPath)
    {
        return ExecuteOnContainerThread(() => LoadFromManifestCore(manifestPath));
    }

    private PluginLoadResult LoadFromManifestCore(string manifestPath)
    {
        if (!RuntimeFeature.IsDynamicCodeSupported)
        {
            SFLog.Warn("PluginLoader: dynamic code is not supported, load request ignored.");
            return PluginLoadResult.FromFailure(PluginLoadFailureReason.DynamicCodeUnsupported,
                "Dynamic code is not supported by the current runtime.");
        }

        PluginManifestResolutionResult manifestResolution = _manifestResolver.Resolve(manifestPath);
        if (!manifestResolution.Success)
        {
            SFLog.Error($"PluginLoader: {manifestResolution.Message}");
            return PluginLoadResult.FromFailure(PluginLoadFailureReason.ManifestResolutionFailed, manifestResolution.Message);
        }

        ResolvedPluginManifest manifest = manifestResolution.Manifest!;
        if (manifest.MinHostVersion is Version required && required > _hostVersion)
        {
            string message = $"Plugin '{manifest.PluginId}' requires host >= {required}, current is {_hostVersion}. Skipping.";
            SFLog.Error($"PluginLoader[{manifest.PluginId}]: {message}");
            return PluginLoadResult.FromFailure(PluginLoadFailureReason.ManifestResolutionFailed, message, manifest.PluginId, manifest);
        }

        lock (_sync)
        {
            if (_plugins.TryGetValue(manifest.PluginId, out LoadedPlugin? existing))
            {
                string message = existing.State == PluginState.Loaded
                    ? $"Plugin '{manifest.PluginId}' is already loaded."
                    : $"Plugin '{manifest.PluginId}' is in state {existing.State} and cannot be loaded again.";
                PluginLoadFailureReason reason = existing.State == PluginState.Loaded ? PluginLoadFailureReason.AlreadyLoaded : PluginLoadFailureReason.Busy;
                SFLog.Error($"PluginLoader[{manifest.PluginId}]: {message}");
                return PluginLoadResult.FromFailure(reason, message, manifest.PluginId, manifest);
            }
        }

        PluginLoadContext context = new(manifest.PluginId, manifest.AssemblyPath);
        Assembly assembly;
        try
        {
            assembly = context.LoadFromAssemblyPath(manifest.AssemblyPath);
            SFLog.Debug($"PluginLoader[{manifest.PluginId}]: loaded assembly {assembly.FullName}");
        }
        catch (Exception ex)
        {
            PluginLoadResult failure = CreateAssemblyLoadFailure(manifest, context, ex);
            SFLog.Error(ex, $"PluginLoader[{manifest.PluginId}]: {failure.Message}");
            TryUnloadContext(context, manifest.PluginId);
            return failure;
        }

        Type[] moduleTypes;
        try
        {
            Type[] allTypes = assembly.GetTypes();
            SFLog.Debug($"PluginLoader[{manifest.PluginId}]: assembly exposes {allTypes.Length} type(s)");
            SFLog.Debug($"PluginLoader[{manifest.PluginId}]: host ISFModule => {PluginSharedAssemblyPolicy.Describe(typeof(ISFModule).Assembly)}");
            SFLog.Debug($"PluginLoader[{manifest.PluginId}]: host SFModuleAttribute => {PluginSharedAssemblyPolicy.Describe(typeof(SFModuleAttribute).Assembly)}");
            foreach (Type t in allTypes)
            {
                bool isClass = !t.IsAbstract && !t.IsInterface;
                bool implISF = typeof(ISFModule).IsAssignableFrom(t);
                bool hasAttr = t.GetCustomAttribute<SFModuleAttribute>() is not null;
                SFLog.Debug($"PluginLoader[{manifest.PluginId}]: type='{t.FullName}' class={isClass} ISFModule={implISF} [SFModule]={hasAttr}");
                foreach (Type iface in t.GetInterfaces())
                {
                    bool sameHostInterface = iface == typeof(ISFModule);
                    SFLog.Debug($"PluginLoader[{manifest.PluginId}]:   iface {iface.FullName} sameHost={sameHostInterface} => {PluginSharedAssemblyPolicy.Describe(iface.Assembly)}");
                }
                foreach (Attribute a in t.GetCustomAttributes(inherit: false))
                {
                    Type at = a.GetType();
                    bool sameHostAttribute = at == typeof(SFModuleAttribute);
                    SFLog.Debug($"PluginLoader[{manifest.PluginId}]:   attr {at.FullName} sameHost={sameHostAttribute} => {PluginSharedAssemblyPolicy.Describe(at.Assembly)}");
                }
            }

            moduleTypes = allTypes
                .Where(type => !type.IsAbstract && !type.IsInterface && type.Assembly == assembly && typeof(ISFModule).IsAssignableFrom(type) && type.GetCustomAttribute<SFModuleAttribute>() is not null)
                .ToArray();
        }
        catch (ReflectionTypeLoadException ex)
        {
            PluginLoadResult failure = CreateTypeEnumerationFailure(manifest, context, ex);
            SFLog.Error(ex, $"PluginLoader[{manifest.PluginId}]: {failure.Message}");
            foreach (Exception? loader in ex.LoaderExceptions)
            {
                if (loader is not null)
                {
                    SFLog.Error(loader, $"PluginLoader[{manifest.PluginId}]: loader exception");
                }
            }

            TryUnloadContext(context, manifest.PluginId);
            return failure;
        }

        if (moduleTypes.Length == 0)
        {
            string message = $"Plugin '{manifest.PluginId}' assembly has no types decorated with [SFModule].";
            SFLog.Warn($"PluginLoader[{manifest.PluginId}]: {message}");
            TryUnloadContext(context, manifest.PluginId);
            return PluginLoadResult.FromFailure(PluginLoadFailureReason.NoModulesFound, message, manifest.PluginId, manifest);
        }

        List<ModuleDescriptor> registered = new(moduleTypes.Length);
        try
        {
            foreach (Type type in moduleTypes)
            {
                ModuleDescriptor descriptor = _container.RegisterOwnedModule(type, manifest.PluginId, manifest.EnabledOnStartOverride);
                registered.Add(descriptor);
                SFLog.Debug($"PluginLoader[{manifest.PluginId}]: registered module id={descriptor.Id} type={type.FullName}");
            }
        }
        catch (Exception ex)
        {
            SFLog.Error(ex, $"PluginLoader[{manifest.PluginId}]: registration failed, rolling back");
            string[] registeredIds = [.. registered.Select(static descriptor => descriptor.Id)];
            if (!_container.TryUnregisterModules(registeredIds, out string[] rollbackFailures) && rollbackFailures.Length != 0)
            {
                SFLog.Error($"PluginLoader[{manifest.PluginId}]: rollback failed for ids=[{string.Join(',', rollbackFailures)}]");
            }

            TryUnloadContext(context, manifest.PluginId);
            return PluginLoadResult.FromFailure(
                PluginLoadFailureReason.ModuleRegistrationFailed,
                $"Plugin '{manifest.PluginId}' failed to register its modules: {ex.GetBaseException().Message}",
                manifest.PluginId,
                manifest);
        }

        LoadedPlugin record = new()
        {
            Manifest = manifest,
            LoadContext = context,
            RegisteredModuleIds = [.. registered.Select(static descriptor => descriptor.Id)],
            LoadContextRef = new WeakReference(context),
        };

        lock (_sync)
        {
            _plugins.Add(manifest.PluginId, record);
        }

        if (_container.IsRunLoopActive)
        {
            int started = _container.ActivatePendingAutoStartModules();
            SFLog.Debug($"PluginLoader[{manifest.PluginId}]: activated {started} pending auto-start module(s) after load");
        }

        SFLog.Info($"PluginLoader[{manifest.PluginId}]: loaded {registered.Count} module(s)");
        return PluginLoadResult.FromSuccess(manifest, registered.Count);
    }

    /// <summary>Stops every module from the plugin, unregisters them, then unloads the ALC.</summary>
    public bool TryUnload(string pluginId)
    {
        return Unload(pluginId).Success;
    }

    public PluginUnloadResult Unload(string pluginId)
    {
        return ExecuteOnContainerThread(() => UnloadCore(pluginId));
    }

    private PluginUnloadResult UnloadCore(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        LoadedPlugin? plugin;
        bool finalizeDetachedUnload = false;
        lock (_sync)
        {
            if (!_plugins.TryGetValue(pluginId, out plugin))
            {
                string message = $"Plugin '{pluginId}' is not loaded.";
                SFLog.Warn($"PluginLoader[{pluginId}]: {message}");
                return PluginUnloadResult.FromFailure(PluginUnloadFailureReason.PluginNotLoaded, message, pluginId);
            }

            if (plugin.State == PluginState.UnloadFailed)
            {
                finalizeDetachedUnload = true;
            }
            else if (plugin.State != PluginState.Loaded)
            {
                string message = $"Plugin '{pluginId}' is in state {plugin.State} and cannot be unloaded.";
                SFLog.Warn($"PluginLoader[{pluginId}]: {message}");
                return PluginUnloadResult.FromFailure(PluginUnloadFailureReason.PluginBusy, message, pluginId);
            }
            else
            {
                plugin.State = PluginState.Unloading;
                plugin.LastUnloadFailureReason = PluginUnloadFailureReason.None;
                plugin.LastUnloadFailureMessage = null;
            }
        }

        LoadedPlugin activePlugin = plugin!;
        if (finalizeDetachedUnload)
        {
            return FinalizeDetachedUnload(activePlugin);
        }

        string[] moduleIds = [.. activePlugin.RegisteredModuleIds];
        string[] blockingDependents = _container.GetRunningDependentModuleIds(moduleIds, excludedModuleIds: moduleIds);
        if (blockingDependents.Length != 0)
        {
            return RecordUnloadFailure(
                activePlugin,
                PluginUnloadFailureReason.ModuleStillRunning,
                $"Plugin '{pluginId}' cannot be unloaded while dependent modules are running: {string.Join(", ", blockingDependents)}");
        }

        SFLog.Info($"PluginLoader[{pluginId}]: unloading {moduleIds.Length} module(s)");
        foreach (string moduleId in moduleIds)
        {
            _container.RequestStopModule(moduleId, ModuleStopReason.PluginUnload);
        }

        if (!_container.TryWaitForModulesStopped(moduleIds, UnloadStopTimeout, out string[] stillRunning))
        {
            return RecordUnloadFailure(
                activePlugin,
                PluginUnloadFailureReason.ModuleStopTimeout,
                $"Plugin '{pluginId}' failed to stop all modules within {UnloadStopTimeout.TotalSeconds:0}s. Still running: {string.Join(", ", stillRunning)}");
        }

        if (!_container.TryUnregisterModules(moduleIds, out string[] failedIds))
        {
            return RecordUnloadFailure(
                activePlugin,
                PluginUnloadFailureReason.ModuleUnregisterFailed,
                $"Plugin '{pluginId}' failed to unregister module ids: {string.Join(", ", failedIds)}");
        }

        if (!TryDetachAndUnloadContext(activePlugin, pluginId))
        {
            return RecordUnloadFailure(
                activePlugin,
                PluginUnloadFailureReason.AssemblyLoadContextUnloadFailed,
                $"Plugin '{pluginId}' failed to invoke AssemblyLoadContext.Unload().");
        }

        return FinalizeDetachedUnload(activePlugin);
    }

    /// <summary>Unload + re-scan the manifest. Convenience wrapper for <c>/sfs reload</c>.</summary>
    public bool TryReload(string pluginId)
    {
        return Reload(pluginId).Success;
    }

    public PluginReloadResult Reload(string pluginId)
    {
        return ExecuteOnContainerThread(() => ReloadCore(pluginId));
    }

    private PluginReloadResult ReloadCore(string pluginId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginId);

        string? manifestPath;
        lock (_sync)
        {
            if (!_plugins.TryGetValue(pluginId, out LoadedPlugin? existing))
            {
                string message = $"Plugin '{pluginId}' is not loaded.";
                SFLog.Warn($"PluginLoader[{pluginId}]: {message}");
                return PluginReloadResult.FromFailure(pluginId, message);
            }

            if (existing.State != PluginState.Loaded)
            {
                string message = $"Plugin '{pluginId}' is in state {existing.State} and cannot be reloaded.";
                SFLog.Warn($"PluginLoader[{pluginId}]: {message}");
                return PluginReloadResult.FromFailure(pluginId, message);
            }

            manifestPath = existing.ManifestPath;
        }

        SFLog.Info($"PluginLoader[{pluginId}]: reload starting");
        PluginUnloadResult unloadResult = UnloadCore(pluginId);
        if (!unloadResult.Success)
        {
            return PluginReloadResult.FromFailure(pluginId, unloadResult.Message, unloadResult);
        }

        PluginLoadResult loadResult = LoadFromManifestCore(manifestPath);
        if (!loadResult.Success)
        {
            return PluginReloadResult.FromFailure(pluginId, loadResult.Message, unloadResult, loadResult);
        }

        return PluginReloadResult.FromSuccess(pluginId, unloadResult, loadResult);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryUnloadContext(PluginLoadContext context, string pluginId)
    {
        try
        {
            context.Unload();
            return true;
        }
        catch (InvalidOperationException ex)
        {
            SFLog.Error(ex, $"PluginLoader[{pluginId}]: ALC.Unload threw");
            return false;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool TryDetachAndUnloadContext(LoadedPlugin plugin, string pluginId)
    {
        PluginLoadContext? context = plugin.DetachLoadContext();
        if (context is null)
        {
            return true;
        }

        return TryUnloadContext(context, pluginId);
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

    private static T ExecuteOnContainerThread<T>(Func<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (SynchronizationContext.Current is SFSynchronizationContext || !SFBootstrap.HasMainThreadDispatcher)
        {
            return action();
        }

        TaskCompletionSource<T> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        SFBootstrap.PostToMainThread(() =>
        {
            try
            {
                tcs.SetResult(action());
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });

        return tcs.Task.GetAwaiter().GetResult();
    }

    private PluginUnloadResult RecordUnloadFailure(LoadedPlugin plugin, PluginUnloadFailureReason reason, string message)
    {
        lock (_sync)
        {
            if (_plugins.TryGetValue(plugin.PluginId, out LoadedPlugin? current) && ReferenceEquals(current, plugin))
            {
                current.State = PluginState.UnloadFailed;
                current.LastUnloadFailureReason = reason;
                current.LastUnloadFailureMessage = message;
            }
        }

        SFLog.Error($"PluginLoader[{plugin.PluginId}]: {message}");
        return PluginUnloadResult.FromFailure(reason, message, plugin.PluginId);
    }

    private PluginUnloadResult FinalizeDetachedUnload(LoadedPlugin plugin)
    {
        bool onMainThread = SynchronizationContext.Current is SFSynchronizationContext;
        for (int attempt = 0; attempt < UnloadGcAttempts && plugin.LoadContextRef.IsAlive; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (onMainThread)
            {
                SFBootstrap.PumpMainThreadQueue();
            }
        }

        if (plugin.LoadContextRef.IsAlive)
        {
            return RecordUnloadFailure(
                plugin,
                PluginUnloadFailureReason.AssemblyLoadContextStillAlive,
                $"Plugin '{plugin.PluginId}' could not be fully unloaded. The plugin AssemblyLoadContext is still alive after {UnloadGcAttempts} GC cycles.");
        }

        lock (_sync)
        {
            _plugins.Remove(plugin.PluginId);
        }

        SFLog.Info($"PluginLoader[{plugin.PluginId}]: ALC fully collected");
        return PluginUnloadResult.FromSuccess(plugin.PluginId);
    }

    private static PluginLoadResult CreateAssemblyLoadFailure(ResolvedPluginManifest manifest, PluginLoadContext context, Exception ex)
    {
        string[] missingNative = context.SnapshotUnresolvedNativeDependencies();
        if (ex is DllNotFoundException || missingNative.Length != 0)
        {
            string suffix = missingNative.Length == 0 ? ex.Message : $"Missing native dependency: {string.Join(", ", missingNative)}.";
            return PluginLoadResult.FromFailure(
                PluginLoadFailureReason.MissingNativeDependency,
                $"Plugin '{manifest.PluginId}' failed to load because a native dependency could not be resolved. {suffix}",
                manifest.PluginId,
                manifest);
        }

        string[] missingManaged = context.SnapshotUnresolvedManagedDependencies();
        if (ex is FileNotFoundException or FileLoadException || missingManaged.Length != 0)
        {
            string suffix = missingManaged.Length == 0 ? ex.Message : $"Missing managed dependency: {string.Join(", ", missingManaged)}.";
            return PluginLoadResult.FromFailure(
                PluginLoadFailureReason.MissingManagedDependency,
                $"Plugin '{manifest.PluginId}' failed to load because a managed dependency could not be resolved. {suffix}",
                manifest.PluginId,
                manifest);
        }

        return PluginLoadResult.FromFailure(
            PluginLoadFailureReason.AssemblyLoadFailed,
            $"Plugin '{manifest.PluginId}' failed to load assembly '{manifest.AssemblyPath}': {ex.GetBaseException().Message}",
            manifest.PluginId,
            manifest);
    }

    private static PluginLoadResult CreateTypeEnumerationFailure(ResolvedPluginManifest manifest, PluginLoadContext context, ReflectionTypeLoadException ex)
    {
        Exception? firstLoaderError = ex.LoaderExceptions.FirstOrDefault(static error => error is not null);
        if (firstLoaderError is DllNotFoundException || context.SnapshotUnresolvedNativeDependencies().Length != 0)
        {
            string[] unresolved = context.SnapshotUnresolvedNativeDependencies();
            string suffix = unresolved.Length == 0 ? firstLoaderError?.Message ?? ex.Message : $"Missing native dependency: {string.Join(", ", unresolved)}.";
            return PluginLoadResult.FromFailure(
                PluginLoadFailureReason.MissingNativeDependency,
                $"Plugin '{manifest.PluginId}' failed during type enumeration because a native dependency could not be resolved. {suffix}",
                manifest.PluginId,
                manifest);
        }

        if (firstLoaderError is FileNotFoundException or FileLoadException || context.SnapshotUnresolvedManagedDependencies().Length != 0)
        {
            string[] unresolved = context.SnapshotUnresolvedManagedDependencies();
            string suffix = unresolved.Length == 0 ? firstLoaderError?.Message ?? ex.Message : $"Missing managed dependency: {string.Join(", ", unresolved)}.";
            return PluginLoadResult.FromFailure(
                PluginLoadFailureReason.MissingManagedDependency,
                $"Plugin '{manifest.PluginId}' failed during type enumeration because a managed dependency could not be resolved. {suffix}",
                manifest.PluginId,
                manifest);
        }

        return PluginLoadResult.FromFailure(
            PluginLoadFailureReason.TypeEnumerationFailed,
            $"Plugin '{manifest.PluginId}' failed to enumerate module types: {ex.GetBaseException().Message}",
            manifest.PluginId,
            manifest);
    }
}
