namespace SFSharp.Runtime.Bootstrap;

/// <summary>
/// Composition root. The core (exception reporting, main-thread queue, network filters and dispatcher) exists
/// from the first <c>WinMainLoop</c> tick; hooks are installed once samp.dll is loaded and the game services
/// (<see cref="Host"/>) are created once CNetGame is ready, because native wrappers resolve samp.dll on first use.
/// </summary>
internal sealed class SFRuntime
{
    private HookRegistry? _hooks;
    private SFHost? _host;

    public SFRuntime()
    {
        Exceptions = new ExceptionReporter();
        Context = new SFSynchronizationContext(Exceptions.Report);
        MainThread = new MainThreadDispatcher(Context);
        Filters = new NetworkFilters();
        Dispatcher = new NetworkDispatcher(MainThread);
        HookRuntime.ReportException = Exceptions.Report;
    }

    public ExceptionReporter Exceptions { get; }
    public SFSynchronizationContext Context { get; }
    public MainThreadDispatcher MainThread { get; }
    public NetworkFilters Filters { get; }
    public NetworkDispatcher Dispatcher { get; }

    public HookRegistry Hooks => _hooks ?? throw new InvalidOperationException("Hooks are installed after samp.dll loads.");
    public SFHost Host => _host ?? throw new InvalidOperationException("Game services are created after CNetGame is ready.");
    public bool HasHost => _host is not null;

    public void InstallEarlyHooks()
    {
        _hooks ??= new HookRegistry(Filters, Dispatcher);
    }

    public void CreateServices()
    {
        _host ??= new SFHost(this);
    }
}
