using System.Diagnostics.CodeAnalysis;
using MinHook;

namespace SFSharp;

public struct NoRetValue;

public interface ISubHook<TArgs, TResult>
{
    TResult Process(TArgs args, Func<TArgs, TResult> next);
}

public abstract class HookBase<TArgs, TResult>
{
    private List<ISubHook<TArgs, TResult>> _subHooks = new();
    private Func<TArgs, TResult> _invokeSubHooks;
    private bool _isProcessing = false;

    protected abstract TResult InvokeOriginalFunction(TArgs args);

    protected HookBase()
    {
        BuildHookChain();
    }

    public void AddSubHook(ISubHook<TArgs, TResult> subHook)
    {
        if (_isProcessing)
        {
            _subHooks = _subHooks.ToList();
        }

        _subHooks.Add(subHook);
        SFLog.Info($"AddSubHook hook={GetType().Name} subHook={subHook.GetType().Name} count={_subHooks.Count}");
        BuildHookChain();
    }

    public void RemoveSubHook(ISubHook<TArgs, TResult> subHook)
    {
        if (_isProcessing)
        {
            _subHooks = _subHooks.ToList();
        }

        _subHooks.Remove(subHook);
        SFLog.Info($"RemoveSubHook hook={GetType().Name} subHook={subHook.GetType().Name} count={_subHooks.Count}");
        BuildHookChain();
    }

    [MemberNotNull(nameof(_invokeSubHooks))]
    protected void BuildHookChain()
    {
        var next = InvokeOriginalFunction;
        foreach (var subHook in _subHooks.AsEnumerable().Reverse())
        {
            var current = next;
            next = args => subHook.Process(args, current);
        }

        _invokeSubHooks = next;
    }

    protected TResult Process(TArgs args)
    {
        _isProcessing = true;
        try
        {
            return _invokeSubHooks(args);
        }
        catch (Exception e)
        {
            SFBootstrap.ProcessException(e);
            return InvokeOriginalFunction(args);
        }
        finally
        {
            _isProcessing = false;
        }
    }
}

public static class HookRuntime
{
    public static HookEngine Engine { get; } = new();
}

public abstract class NativeHook<TArgs, TResult, TDelegate> : HookBase<TArgs, TResult>, IDisposable
    where TDelegate : Delegate
{
    private sealed class HookSuppressionScope : IDisposable
    {
        private NativeHook<TArgs, TResult, TDelegate>? _owner;

        public HookSuppressionScope(NativeHook<TArgs, TResult, TDelegate> owner)
        {
            _owner = owner;
            HookRuntime.Engine.DisableHook(owner.OriginalFunction);
        }

        public void Dispose()
        {
            if (_owner is null)
            {
                return;
            }

            HookRuntime.Engine.EnableHook(_owner.OriginalFunction);
            _owner = null;
        }
    }

    private TDelegate? _detour;
    private bool _isInstalled;

    protected TDelegate OriginalFunction { get; private set; } = null!;
    protected nint TargetAddress { get; private set; }

    protected void InstallHook(nint targetAddress, TDelegate detour)
    {
        if (_isInstalled)
        {
            throw new InvalidOperationException("Hook already installed");
        }

        TargetAddress = targetAddress;
        _detour = detour;
        SFLog.Info($"InstallHook type={GetType().Name} target=0x{targetAddress:X8} detour={detour.Method.Name}");
        OriginalFunction = HookRuntime.Engine.CreateHook((IntPtr)targetAddress, detour);
        HookRuntime.Engine.EnableHook(OriginalFunction);
        _isInstalled = true;
        SFLog.Info($"InstallHook completed type={GetType().Name} target=0x{targetAddress:X8}");
    }

    protected IDisposable SuppressHook()
    {
        if (!_isInstalled)
        {
            throw new InvalidOperationException("Hook is not installed");
        }

        return new HookSuppressionScope(this);
    }

    public virtual void Dispose()
    {
        if (!_isInstalled)
        {
            return;
        }

        HookRuntime.Engine.DisableHook(OriginalFunction);
        SFLog.Info($"Dispose hook type={GetType().Name} target=0x{TargetAddress:X8}");
        _isInstalled = false;
        _detour = null;
    }
}
