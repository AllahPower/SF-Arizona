namespace SFSharp.Runtime.Modules.Hosting;

/// <summary>Owns exactly one run; telemetry never owns plugin resources or tasks.</summary>
internal sealed class ModuleRunScope : IDisposable
{
    private readonly Lock _sync = new();
    private readonly CancellationTokenSource _cancellation;
    private readonly List<IDisposable> _resources = [];
    private readonly HashSet<Task> _tasks = [];
    private readonly Action<int, int> _countsChanged;
    private readonly Action<Exception> _report;
    private Exception? _failure;
    private bool _closed;
    private bool _disposed;

    public ModuleRunScope(CancellationToken token, Action<int, int> countsChanged, Action<Exception> report)
    {
        _cancellation = CancellationTokenSource.CreateLinkedTokenSource(token);
        Token = _cancellation.Token;
        _countsChanged = countsChanged;
        _report = report;
    }

    public CancellationToken Token { get; }

    public IDisposable Register(IDisposable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        lock (_sync)
        {
            if (!_closed && !Token.IsCancellationRequested)
            {
                _resources.Add(resource);
                PublishCounts();
                return resource;
            }
        }

        resource.Dispose();
        throw new ObjectDisposedException(nameof(ModuleRunScope), "The module run is stopping.");
    }

    public Task Run(Func<CancellationToken, Task> work, bool background)
    {
        ArgumentNullException.ThrowIfNull(work);
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_closed || Token.IsCancellationRequested, this);
            _tasks.Add(completion.Task);
            PublishCounts();
        }

        if (background) _ = Task.Run(() => ExecuteAsync(work, completion));
        else _ = ExecuteAsync(work, completion);
        return completion.Task;
    }

    private async Task ExecuteAsync(Func<CancellationToken, Task> work, TaskCompletionSource completion)
    {
        Exception? failure = null;
        bool cancelled = false;
        try
        {
            Token.ThrowIfCancellationRequested();
            await work(Token);
        }
        catch (OperationCanceledException) when (Token.IsCancellationRequested) { cancelled = true; }
        catch (Exception ex) { failure = ex; }

        // Record failures before removing a task, even when no caller awaits its returned handle.
        if (failure is not null)
        {
            lock (_sync) _failure ??= failure;
            Report(failure);
            BeginStop();
        }

        lock (_sync)
        {
            if (failure is not null)
            {
                completion.TrySetException(failure);
                _ = completion.Task.Exception;
            }
            else if (cancelled) completion.TrySetCanceled(Token);
            else completion.TrySetResult();
            _tasks.Remove(completion.Task);
            PublishCounts();
        }
    }

    public void BeginStop()
    {
        lock (_sync)
        {
            if (_closed) return;
            _closed = true;
        }

        try { _cancellation.Cancel(); }
        catch (Exception ex)
        {
            lock (_sync) _failure ??= ex;
            Report(ex);
        }
    }

    public async Task DrainAsync()
    {
        BeginStop();
        Task[] tasks;
        lock (_sync) tasks = [.. _tasks];
        try { await Task.WhenAll(tasks); }
        catch { /* Every task outcome is recorded by ExecuteAsync before it completes. */ }

        Exception? failure;
        lock (_sync) failure = _failure;
        if (failure is not null) System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
    }

    public void Dispose()
    {
        BeginStop();
        IDisposable[] resources;
        lock (_sync)
        {
            if (_disposed) return;
            if (_tasks.Count != 0)
                throw new InvalidOperationException("Module work must finish before resources can be released.");
            _disposed = true;
            resources = [.. _resources];
            _resources.Clear();
            PublishCounts();
        }

        for (int i = resources.Length - 1; i >= 0; i--)
        {
            try { resources[i].Dispose(); }
            catch (Exception ex) { Report(ex); }
        }
        _cancellation.Dispose();
    }

    private void PublishCounts() => _countsChanged(_resources.Count, _tasks.Count);

    private void Report(Exception exception)
    {
        try { _report(exception); }
        catch { /* A broken sink must not interrupt module cleanup. */ }
    }
}
