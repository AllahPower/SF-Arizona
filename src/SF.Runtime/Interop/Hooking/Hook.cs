using MinHook;
using System.Runtime.ExceptionServices;

namespace SFSharp.Runtime.Interop.Hooking;

public struct NoRetValue;

public interface ISubHook<TArgs, TResult>
{
    TResult Process(TArgs args, Func<TArgs, TResult> next);
}

public abstract class HookBase<TArgs, TResult>
{
    private readonly Lock _sync = new();
    private readonly List<ISubHook<TArgs, TResult>> _subHooks = [];
    private Func<TArgs, TResult> _invokeSubHooks;

    [ThreadStatic] private static Invocation? _current;
    [ThreadStatic] private static Invocation? _available;

    private sealed class Invocation
    {
        public HookBase<TArgs, TResult>? Owner;
        public Invocation? Next;
        public bool OriginalStarted;
        public TResult Result = default!;
        public TResult ChainResult = default!;
        public bool HasChainResult;
        public ExceptionDispatchInfo? Failure;
    }

    protected abstract TResult InvokeOriginalFunction(TArgs args);

    protected HookBase()
    {
        _invokeSubHooks = InvokeOriginalOnce;
    }

    public void AddSubHook(ISubHook<TArgs, TResult> subHook)
    {
        ArgumentNullException.ThrowIfNull(subHook);
        lock (_sync)
        {
            _subHooks.Add(subHook);
            BuildHookChain();
        }
    }

    public void RemoveSubHook(ISubHook<TArgs, TResult> subHook)
    {
        lock (_sync)
        {
            _subHooks.Remove(subHook);
            BuildHookChain();
        }
    }

    private void BuildHookChain()
    {
        Func<TArgs, TResult> next = InvokeOriginalOnce;
        foreach (var subHook in _subHooks.AsEnumerable().Reverse())
        {
            var current = next;
            next = args =>
            {
                TResult result = subHook.Process(args, current);
                Invocation invocation = _current!;
                invocation.ChainResult = result;
                invocation.HasChainResult = true;
                return result;
            };
        }

        Volatile.Write(ref _invokeSubHooks, next);
    }

    private TResult InvokeOriginalOnce(TArgs args)
    {
        Invocation invocation = _current ?? throw new InvalidOperationException("Hook continuation called outside its invocation.");
        if (!ReferenceEquals(invocation.Owner, this))
            throw new InvalidOperationException("Hook continuation belongs to another invocation.");

        if (!invocation.OriginalStarted)
        {
            invocation.OriginalStarted = true;
            try { invocation.Result = InvokeOriginalFunction(args); }
            catch (Exception ex) { invocation.Failure = ExceptionDispatchInfo.Capture(ex); }
        }

        invocation.Failure?.Throw();
        return invocation.Result;
    }

    protected TResult Process(TArgs args)
    {
        // Reuse frames per thread and nesting depth, without retaining callback results between calls.
        Invocation invocation = _available ?? new Invocation();
        _available = invocation.Next;
        invocation.Next = _current;
        invocation.Owner = this;
        _current = invocation;
        try
        {
            return Volatile.Read(ref _invokeSubHooks)(args);
        }
        catch (Exception e)
        {
            HookRuntime.ReportSafely(e);
            invocation.Failure?.Throw();
            if (invocation.HasChainResult) return invocation.ChainResult;
            return InvokeOriginalOnce(args);
        }
        finally
        {
            _current = invocation.Next;
            invocation.Owner = null;
            invocation.Result = default!;
            invocation.ChainResult = default!;
            invocation.HasChainResult = false;
            invocation.Failure = null;
            invocation.OriginalStarted = false;
            invocation.Next = _available;
            _available = invocation;
        }
    }
}

public static class HookRuntime
{
    private static readonly Lazy<HookEngine> NativeEngine = new(static () => new());
    public static HookEngine Engine => NativeEngine.Value;

    /// <summary>
    /// Receives exceptions thrown by sub-hooks inside native callbacks, which have no caller to propagate to.
    /// The composition root points it at the runtime's exception reporter.
    /// </summary>
    public static Action<Exception> ReportException { get; set; } = static ex => SFLog.Error(ex, "Unhandled hook exception");

    internal static void ReportSafely(Exception exception)
    {
        try { ReportException(exception); }
        catch { /* Exception reporting must never escape a native callback. */ }
    }
}

public abstract class NativeHook<TArgs, TResult, TDelegate> : HookBase<TArgs, TResult>, IDisposable
    where TDelegate : Delegate
{
    private TDelegate? _detour;
    private bool _isInstalled;

    protected TDelegate OriginalFunction { get; private set; } = null!;
    protected nint TargetAddress { get; private set; }

    protected TResult ProcessNative(TArgs args, TResult failureResult)
    {
        try { return Process(args); }
        catch (Exception ex)
        {
            HookRuntime.ReportSafely(ex);
            return failureResult;
        }
    }

    protected void InstallHook(nint targetAddress, TDelegate detour)
    {
        if (_detour is not null)
        {
            throw new InvalidOperationException("Hook already installed");
        }

        TargetAddress = targetAddress;
        _detour = detour;
        SFLog.Debug($"InstallHook type={GetType().Name} target=0x{targetAddress:X8} detour={detour.Method.Name}");
        OriginalFunction = HookRuntime.Engine.CreateHook((IntPtr)targetAddress, detour);
        HookRuntime.Engine.EnableHook(OriginalFunction);
        _isInstalled = true;
        SFLog.Debug($"InstallHook completed type={GetType().Name} target=0x{targetAddress:X8}");
    }

    public virtual void Dispose()
    {
        if (!_isInstalled)
        {
            return;
        }

        HookRuntime.Engine.DisableHook(OriginalFunction);
        SFLog.Debug($"Dispose hook type={GetType().Name} target=0x{TargetAddress:X8}");
        _isInstalled = false;
        // Trampoline and callback ownership remain resident; disabling is not native reclamation.
    }
}