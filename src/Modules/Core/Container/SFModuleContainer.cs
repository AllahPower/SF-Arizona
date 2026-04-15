namespace SFSharp;

/// <summary>
/// Registers and runs SF modules. One container instance typically lives for the whole lifetime
/// of the game process and is driven from <c>Program.Main</c>.
/// </summary>
/// <remarks>
/// <para>
/// Registration happens before <see cref="Run(CancellationToken)"/> via
/// <see cref="RegisterModule{T}(bool?)"/>. Inside <c>Run</c> the container starts every module whose
/// <see cref="ModuleDescriptor.DefaultEnabled"/> or override is <see langword="true"/>, then waits
/// for modules to complete or for new modules to be started from the <c>/sfs</c> dashboard.
/// </para>
/// <para>
/// Fault handling is driven by <see cref="ModuleRestartPolicy"/>. A circuit breaker trips after
/// 5 faults in a rolling 60 second window and disables auto-restart for that module until the
/// user re-enables it from the dashboard.
/// </para>
/// <para>
/// This class is split across two files: this file carries registration and lifecycle, the
/// <c>SFModuleContainer.Dashboard</c> file carries the <c>/sfs</c> chat command and the dialog UI.
/// </para>
/// </remarks>
public sealed partial class SFModuleContainer
{
    /// <summary>
    /// Internal registration record. Holds the descriptor, the factory, the per-module runtime
    /// telemetry object and the current auto-start flag.
    /// </summary>
    private sealed class ModuleRegistration(ModuleDescriptor descriptor, Func<ISFModule> factory, bool autoStartEnabled, string? ownerPluginId)
    {
        public ModuleDescriptor Descriptor { get; } = descriptor;
        public Func<ISFModule> Factory { get; } = factory;
        public ModuleRuntimeInfo Runtime { get; } = new(descriptor, autoStartEnabled);
        public bool AutoStartEnabled { get; set; } = autoStartEnabled;
        public string? OwnerPluginId { get; } = ownerPluginId;
    }

    /// <summary>Tracks a currently running module instance and the resources tied to its run.</summary>
    private sealed record RunningModule(ISFModule Instance, ModuleContext Context, Task Task, CancellationTokenSource CancellationTokenSource);

    private readonly List<ModuleRegistration> _registrations = [];
    private readonly Dictionary<string, ModuleRegistration> _registrationsById = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<ModuleRegistration, RunningModule> _runningModules = [];
    private PluginLoader? _pluginLoader;

    /// <summary>
    /// Attached plugin loader, if any. Set by <c>Program.Main</c> after the loader is constructed.
    /// Used by the <c>/sfs</c> dashboard to expose <c>plugin-load</c>/<c>plugin-unload</c>/<c>plugin-reload</c>.
    /// </summary>
    public PluginLoader? PluginLoader
    {
        get => _pluginLoader;
        set => _pluginLoader = value;
    }
    private TaskCompletionSource _moduleStartTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private Task[]? _cachedWhenAnyTasks;
    private bool _isShuttingDown;

    public SFModuleContainer()
    {
        PublishModuleCatalogSnapshot();
    }

    /// <summary>
    /// Registers a module type with the container. Must be called before
    /// <see cref="Run(CancellationToken)"/>. The type needs a parameterless constructor and a
    /// <see cref="SFModuleAttribute"/> annotation.
    /// </summary>
    /// <typeparam name="T">Concrete module type implementing <see cref="ISFModule"/>.</typeparam>
    /// <param name="enabledOnStart">
    /// When <see langword="null"/>, uses <see cref="SFModuleAttribute.DefaultEnabled"/>. Pass
    /// <see langword="true"/>/<see langword="false"/> to override the default.
    /// </param>
    /// <exception cref="InvalidOperationException">A module with the same <see cref="ModuleDescriptor.Id"/> is already registered.</exception>
    public void RegisterModule<T>(bool? enabledOnStart = null) where T : ISFModule, new()
    {
        RegisterCore(ModuleDescriptor.FromType(typeof(T)), static () => new T(), enabledOnStart, ownerPluginId: null);
    }

    /// <summary>
    /// Reflection-based overload used by <see cref="PluginLoader"/> for types discovered at runtime.
    /// The type must implement <see cref="ISFModule"/>, carry <see cref="SFModuleAttribute"/> and
    /// expose a public parameterless constructor.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode("Activator.CreateInstance relies on reflection and is unavailable under NativeAOT.")]
    public void RegisterModule(Type moduleType, bool? enabledOnStart = null)
    {
        _ = RegisterOwnedModule(moduleType, ownerPluginId: null, enabledOnStart);
    }

    internal ModuleDescriptor RegisterOwnedModule(Type moduleType, string? ownerPluginId, bool? enabledOnStart = null)
    {
        ArgumentNullException.ThrowIfNull(moduleType);
        if (!typeof(ISFModule).IsAssignableFrom(moduleType))
        {
            throw new ArgumentException($"Type {moduleType.FullName} does not implement ISFModule.", nameof(moduleType));
        }

        if (moduleType.GetConstructor(Type.EmptyTypes) is null)
        {
            throw new ArgumentException($"Type {moduleType.FullName} has no public parameterless constructor.", nameof(moduleType));
        }

        ModuleDescriptor descriptor = ModuleDescriptor.FromType(moduleType);
        RegisterCore(descriptor, () => (ISFModule)Activator.CreateInstance(moduleType)!, enabledOnStart, ownerPluginId);
        return descriptor;
    }

    private void RegisterCore(ModuleDescriptor descriptor, Func<ISFModule> factory, bool? enabledOnStart, string? ownerPluginId)
    {
        if (_registrationsById.ContainsKey(descriptor.Id))
        {
            throw new InvalidOperationException($"Module id '{descriptor.Id}' is already registered.");
        }

        bool autoStartEnabled = enabledOnStart ?? descriptor.DefaultEnabled;
        ModuleRegistration registration = new(descriptor, factory, autoStartEnabled, ownerPluginId);
        _registrations.Add(registration);
        _registrations.Sort((left, right) =>
        {
            int order = left.Descriptor.Order.CompareTo(right.Descriptor.Order);
            return order != 0 ? order : string.Compare(left.Descriptor.DisplayName, right.Descriptor.DisplayName, StringComparison.OrdinalIgnoreCase);
        });
        _registrationsById.Add(descriptor.Id, registration);
        SFLog.Info($"RegisterModule id={descriptor.Id} type={descriptor.ModuleType.Name} enabledOnStart={autoStartEnabled} execution={descriptor.ExecutionModel}");
        PublishModuleCatalogSnapshot();
    }

    /// <summary>
    /// Removes a registration after its run has ended. If the module is still running the call
    /// returns <see langword="false"/> — call <see cref="RequestStopModule(string, ModuleStopReason)"/>
    /// and <see cref="WaitForModulesStopped(IEnumerable{string}, TimeSpan)"/> first.
    /// </summary>
    public bool TryUnregisterModule(string moduleId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);
        if (!_registrationsById.TryGetValue(moduleId, out ModuleRegistration? registration))
        {
            SFLog.Warn($"TryUnregisterModule id={moduleId}: not registered");
            return false;
        }

        if (_runningModules.ContainsKey(registration))
        {
            SFLog.Warn($"TryUnregisterModule id={moduleId}: still running, refuse to remove");
            return false;
        }

        _registrationsById.Remove(moduleId);
        _registrations.Remove(registration);
        SFLog.Info($"UnregisterModule id={moduleId}");
        PublishModuleCatalogSnapshot();
        return true;
    }

    /// <summary>Requests cooperative stop of a module by id. No-op if unknown or already stopped.</summary>
    public void RequestStopModule(string moduleId, ModuleStopReason reason)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);
        if (!_registrationsById.TryGetValue(moduleId, out ModuleRegistration? registration))
        {
            return;
        }

        registration.AutoStartEnabled = false;
        registration.Runtime.SetAutoStartEnabled(false);
        StopModule(registration, reason);
    }

    /// <summary>
    /// Blocks the calling thread until every listed module is no longer running or until
    /// <paramref name="timeout"/> elapses. Used by <see cref="PluginLoader"/> to drain tasks
    /// before tearing down the ALC.
    /// </summary>
    public bool TryWaitForModulesStopped(IEnumerable<string> moduleIds, TimeSpan timeout, out string[] stillRunningIds)
    {
        ArgumentNullException.ThrowIfNull(moduleIds);
        string[] ids = moduleIds.Where(static id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (ids.Length == 0)
        {
            stillRunningIds = [];
            return true;
        }

        DateTime deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            stillRunningIds = GetRunningModuleIds(ids);
            if (stillRunningIds.Length == 0)
            {
                return true;
            }

            Thread.Sleep(50);
        }

        stillRunningIds = GetRunningModuleIds(ids);
        SFLog.Warn($"WaitForModulesStopped: timeout after {timeout.TotalMilliseconds:F0}ms, ids=[{string.Join(',', ids)}] stillRunning=[{string.Join(',', stillRunningIds)}]");
        return stillRunningIds.Length == 0;
    }

    public void WaitForModulesStopped(IEnumerable<string> moduleIds, TimeSpan timeout)
    {
        _ = TryWaitForModulesStopped(moduleIds, timeout, out _);
    }

    public bool TryUnregisterModules(IEnumerable<string> moduleIds, out string[] failedIds)
    {
        ArgumentNullException.ThrowIfNull(moduleIds);
        string[] ids = moduleIds.Where(static id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (ids.Length == 0)
        {
            failedIds = [];
            return true;
        }

        List<(string Id, ModuleRegistration Registration)> registrations = new(ids.Length);
        List<string> failures = [];
        foreach (string id in ids)
        {
            if (!_registrationsById.TryGetValue(id, out ModuleRegistration? registration) || _runningModules.ContainsKey(registration))
            {
                failures.Add(id);
                continue;
            }

            registrations.Add((id, registration));
        }

        if (failures.Count != 0)
        {
            failedIds = [.. failures];
            return false;
        }

        foreach ((string id, ModuleRegistration registration) in registrations)
        {
            _registrationsById.Remove(id);
            _registrations.Remove(registration);
            SFLog.Info($"UnregisterModule id={id}");
        }

        PublishModuleCatalogSnapshot();
        failedIds = [];
        return true;
    }

    private void InvalidateTaskCache() => _cachedWhenAnyTasks = null;

    private Task[] GetWhenAnyTasks()
    {
        return _cachedWhenAnyTasks ??= _runningModules.Values.Select(x => x.Task).Append(_moduleStartTaskSource.Task).ToArray();
    }

    /// <summary>
    /// Starts enabled modules and drives the supervisor loop until <paramref name="token"/> is
    /// cancelled. Returns only after every running module has torn down. Also registers the
    /// <c>/sfs</c> chat command for the lifetime of the loop.
    /// </summary>
    /// <param name="token">Cancellation for the whole container, cascades to every running module.</param>
    public async Task Run(CancellationToken token = default)
    {
        SFLog.Info("SFModuleContainer.Run started");
        foreach (ModuleRegistration registration in _registrations)
        {
            if (!registration.AutoStartEnabled)
            {
                continue;
            }

            StartModule(registration);
        }

        using IDisposable commandRegistration = SF.Chat.RegisterChatCommand("sfs", OnCommand);

        try
        {
            while (!token.IsCancellationRequested)
            {
                Task moduleStartTask = _moduleStartTaskSource.Task;
                Task[] tasks = GetWhenAnyTasks();
                Task completedTask = await Task.WhenAny(tasks);

                if (completedTask == moduleStartTask)
                {
                    _moduleStartTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    InvalidateTaskCache();
                    continue;
                }

                KeyValuePair<ModuleRegistration, RunningModule> completed = _runningModules.Single(x => x.Value.Task == completedTask);
                _runningModules.Remove(completed.Key);
                InvalidateTaskCache();
                await HandleModuleCompletion(completed.Key, completed.Value);
            }
        }
        finally
        {
            _isShuttingDown = true;
            foreach ((ModuleRegistration registration, RunningModule running) in _runningModules.ToArray())
            {
                StopModule(registration, ModuleStopReason.ContainerShutdown);
            }

            if (_runningModules.Count > 0)
            {
                await Task.WhenAll(_runningModules.Values.Select(x => x.Task));
            }
        }
    }

    /// <summary>
    /// Starts a module if it is not already running. Creates a new <see cref="ModuleContext"/>
    /// and dispatches the task according to <see cref="ModuleExecutionModel"/>.
    /// </summary>
    private void StartModule(ModuleRegistration registration)
    {
        if (_runningModules.ContainsKey(registration))
        {
            return;
        }

        registration.Runtime.SetAutoStartEnabled(registration.AutoStartEnabled);
        registration.Runtime.PrepareForStart(registration.AutoStartEnabled);
        ISFModule module = registration.Factory();
        CancellationTokenSource cts = new();
        ModuleContext context = new(registration.Descriptor, registration.Runtime, cts.Token);

        SFLog.Info($"StartModule id={registration.Descriptor.Id} type={registration.Descriptor.ModuleType.Name} execution={registration.Descriptor.ExecutionModel}");
        Task task = registration.Descriptor.ExecutionModel switch
        {
            ModuleExecutionModel.BackgroundWorker => Task.Run(() => RunModuleCore(registration, module, context), cts.Token),
            _ => RunModuleCore(registration, module, context)
        };

        _runningModules.Add(registration, new(module, context, task, cts));
        InvalidateTaskCache();
        _moduleStartTaskSource.TrySetResult();
        _moduleStartTaskSource = new(TaskCreationOptions.RunContinuationsAsynchronously);
        PublishModuleCatalogSnapshot();
    }

    /// <summary>
    /// Invokes <see cref="ISFModule.RunAsync(ModuleContext)"/> under a try/catch that translates
    /// the outcome into the corresponding <see cref="ModuleRuntimeInfo"/> state transition.
    /// </summary>
    private async Task RunModuleCore(ModuleRegistration registration, ISFModule module, ModuleContext context)
    {
        registration.Runtime.MarkRunning();
        PublishModuleCatalogSnapshot();
        try
        {
            await module.RunAsync(context);
            await context.DrainBackgroundTasksAsync();
            registration.Runtime.MarkCompleted();
            PublishModuleCatalogSnapshot();
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            await context.DrainBackgroundTasksAsync();
            registration.Runtime.MarkCompleted();
            PublishModuleCatalogSnapshot();
        }
        catch (Exception ex)
        {
            registration.Runtime.MarkFault(ex);
            PublishModuleCatalogSnapshot();
            throw;
        }
        finally
        {
            context.Dispose();
        }
    }

    /// <summary>Requests cooperative cancellation of a running module. No-op if it is not running.</summary>
    private void StopModule(ModuleRegistration registration, ModuleStopReason reason)
    {
        if (!_runningModules.TryGetValue(registration, out RunningModule? running))
        {
            return;
        }

        SFLog.Info($"StopModule id={registration.Descriptor.Id} reason={reason}");
        registration.Runtime.RequestStop(reason);
        running.CancellationTokenSource.Cancel();
        PublishModuleCatalogSnapshot();
    }

    /// <summary>
    /// Awaits the completed module task, emits a chat notification and applies the restart
    /// policy including the circuit breaker logic.
    /// </summary>
    private async Task HandleModuleCompletion(ModuleRegistration registration, RunningModule running)
    {
        try
        {
            await running.Task;
            ModuleRuntimeSnapshot snapshot = registration.Runtime.CreateSnapshot();
            SFLog.Info($"Module completed id={registration.Descriptor.Id} state={snapshot.State} stopReason={snapshot.LastStopReason}");
            if (snapshot.LastStopReason == ModuleStopReason.Faulted)
            {
                SF.Chat.Add(ModuleChatFormatter.FormatChatAction("faulted", registration.Descriptor.DisplayName, "faulted", SFColors.Red));
            }
            else
            {
                SF.Chat.Add(ModuleChatFormatter.FormatChatAction("stopped", registration.Descriptor.DisplayName, snapshot.LastStopReason.ToString(), SFColors.Orange));
            }
        }
        catch (Exception ex)
        {
            SFLog.Error(ex.GetBaseException(), $"Module faulted id={registration.Descriptor.Id}");
            Exception baseException = ex.GetBaseException();
            SF.Chat.Add(ModuleChatFormatter.FormatChatAction("faulted", registration.Descriptor.DisplayName, "exception", SFColors.Red));
            SF.Chat.Add((SFColors.Rose | SFColors.White).Apply($"{baseException.GetType().Name}: {baseException.Message}"));
        }
        finally
        {
            running.CancellationTokenSource.Dispose();
        }

        ModuleRuntimeSnapshot finalSnapshot = registration.Runtime.CreateSnapshot();
        if (!_isShuttingDown && finalSnapshot.LastStopReason == ModuleStopReason.RestartRequested)
        {
            registration.Runtime.IncrementRestartCount();
            StartModule(registration);
            return;
        }

        if (!_isShuttingDown && finalSnapshot.LastStopReason == ModuleStopReason.Faulted && registration.Descriptor.RestartPolicy == ModuleRestartPolicy.OnFault)
        {
            registration.Runtime.RecordFaultForCircuitBreaker();
            if (registration.Runtime.IsCircuitBroken)
            {
                registration.AutoStartEnabled = false;
                registration.Runtime.SetAutoStartEnabled(false);
                SFLog.Error($"Circuit breaker tripped for module id={registration.Descriptor.Id}: too many faults in 60s, auto-restart disabled");
                SF.Chat.Add(ModuleChatFormatter.FormatChatAction("disabled", registration.Descriptor.DisplayName, "too many faults, auto-restart off", SFColors.Red));
                PublishModuleCatalogSnapshot();
                return;
            }

            registration.Runtime.IncrementRestartCount();
            StartModule(registration);
        }
    }

    /// <summary>Restart: stop a running module so the completion handler relaunches it, or start it directly if it is already stopped.</summary>
    private void RestartModule(ModuleRegistration registration)
    {
        if (_runningModules.ContainsKey(registration))
        {
            StopModule(registration, ModuleStopReason.RestartRequested);
            return;
        }

        registration.Runtime.IncrementRestartCount();
        StartModule(registration);
    }

    /// <summary>
    /// Looks up a registration by exact id, exact display name, exact type name, or case-insensitive
    /// substring match against id and display name. Used by the <c>/sfs</c> command resolver.
    /// </summary>
    private ModuleRegistration? ResolveModule(string query)
    {
        if (_registrationsById.TryGetValue(query, out ModuleRegistration? exact))
        {
            return exact;
        }

        string normalized = query.Trim();
        return _registrations.FirstOrDefault(x =>
            string.Equals(x.Descriptor.DisplayName, normalized, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.Descriptor.ModuleType.Name, normalized, StringComparison.OrdinalIgnoreCase) ||
            x.Descriptor.Id.Contains(normalized, StringComparison.OrdinalIgnoreCase) ||
            x.Descriptor.DisplayName.Contains(normalized, StringComparison.OrdinalIgnoreCase));
    }

    private string[] GetRunningModuleIds(IEnumerable<string> moduleIds)
    {
        List<string> running = [];
        foreach (string id in moduleIds)
        {
            if (_registrationsById.TryGetValue(id, out ModuleRegistration? registration) && _runningModules.ContainsKey(registration))
            {
                running.Add(id);
            }
        }

        return [.. running];
    }

    internal void PublishModuleCatalogSnapshot()
    {
        SFPublicModules.Instance.Publish(_registrations
            .Select(CreatePublicModuleInfo)
            .OrderBy(static module => module.Order)
            .ThenBy(static module => module.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToArray());
    }

    private static SFModuleInfo CreatePublicModuleInfo(ModuleRegistration registration)
    {
        ModuleRuntimeSnapshot runtime = registration.Runtime.CreateSnapshot();
        return new(
            registration.Descriptor.Id,
            registration.Descriptor.DisplayName,
            registration.Descriptor.Category,
            registration.Descriptor.Description,
            registration.Descriptor.DefaultEnabled,
            registration.AutoStartEnabled,
            registration.Descriptor.ExecutionModel,
            registration.Descriptor.RestartPolicy,
            registration.Descriptor.Order,
            Array.AsReadOnly(registration.Descriptor.Dependencies.ToArray()),
            registration.OwnerPluginId,
            runtime.State,
            runtime.LastStopReason,
            runtime.RestartCount,
            runtime.FaultCount);
    }
}
