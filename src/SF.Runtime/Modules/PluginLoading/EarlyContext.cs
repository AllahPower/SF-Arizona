using Microsoft.Extensions.Logging;

namespace SFSharp.Runtime.Modules.PluginLoading;

/// <summary>
/// <see cref="ISFEarlyContext"/> for one early module. Owns every stage subscription and registered disposable
/// so the plugin's load context holds no live references after <see cref="Dispose"/>.
/// </summary>
internal sealed class EarlyContext : ISFEarlyContext, ISFGameLoading, IDisposable
{
    private readonly ISFGameLoading _loading;
    private readonly List<IDisposable> _owned = [];
    private readonly object _sync = new();
    private bool _disposed;

    public EarlyContext(string pluginId, Type moduleType, ISFGameLoading loading)
    {
        PluginId = pluginId;
        _loading = loading;
        Log = SFLoggerProvider.Instance.CreateLogger($"{pluginId}/{moduleType.Name}");
    }

    public string PluginId { get; }
    public ILogger Log { get; }
    public ISFGameLoading Loading => this;

    SFGameLoadStage ISFGameLoading.Stage => _loading.Stage;
    int ISFGameLoading.GameState => _loading.GameState;

    IDisposable ISFGameLoading.Subscribe(SFGameLoadStage stage, Action handler)
        => RegisterDisposable(_loading.Subscribe(stage, handler));

    Task ISFGameLoading.WhenStageAsync(SFGameLoadStage stage) => _loading.WhenStageAsync(stage);

    public IDisposable RegisterDisposable(IDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);
        lock (_sync)
        {
            if (!_disposed)
            {
                _owned.Add(disposable);
                return disposable;
            }
        }

        disposable.Dispose();
        throw new ObjectDisposedException(nameof(EarlyContext), $"Plugin '{PluginId}' is unloading.");
    }

    public void Dispose()
    {
        IDisposable[] owned;
        lock (_sync)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            owned = [.. _owned];
            _owned.Clear();
        }

        for (int i = owned.Length - 1; i >= 0; i--)
        {
            try
            {
                owned[i].Dispose();
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, $"EarlyContext[{PluginId}]: disposing an owned resource failed");
            }
        }
    }
}
