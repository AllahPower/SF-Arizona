using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

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
        if (_isProcessing) _subHooks = _subHooks.ToList();

        _subHooks.Add(subHook);
        BuildHookChain();
    }
    public void RemoveSubHook(ISubHook<TArgs, TResult> subHook)
    {
        if (_isProcessing) _subHooks = _subHooks.ToList();

        _subHooks.Remove(subHook);
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
            return InvokeOriginalFunction(args); // If this fails, we're fucked anyway.
            // Potential bug: a sub-hook may throw after invoking, leading to double invocation.
            // We could inject our own sub-hook, check if it intercepted a return value yet, and if so, return that.
            // It's a bit of an overkill since that's a lot of extra logic just to gracefully handle exceptions in sub-hooks...
            // TODO: do that ^
        }
        finally
        {
            _isProcessing = false;
        }
    }
}

// These hooks used to inherit directly from HookBase, and declared UnmanagedCallersOnly methods.
// As much as I dislike runtime-created thunks from GetFunctionPointerForDelegate, this is much more reusable and less error-prone.
public abstract class JumpHook<TArgs, TResult, TFunction> : HookBase<TArgs, TResult>, IDisposable
    where TFunction : Delegate
{
    protected abstract TFunction HookedFunction { get; }
    protected TFunction Trampoline { get; }

    private readonly uint _stolenByteCount;
    private readonly uint _functionAddress;
    private readonly uint _trampolineAddress;
    private readonly GCHandle _gcHandle;

    protected JumpHook(uint stolenByteCount, string targetFunctionModule, uint targetFunctionOffset)
    {
        _stolenByteCount = stolenByteCount;
        _functionAddress = HookHelper.GetFunctionPtr(targetFunctionModule, targetFunctionOffset);

        _trampolineAddress = HookHelper.InstallJumpHook(
            _functionAddress,
            _stolenByteCount,
            (uint)Marshal.GetFunctionPointerForDelegate(HookedFunction)
        );
        Trampoline = Marshal.GetDelegateForFunctionPointer<TFunction>((nint)_trampolineAddress);

        _gcHandle = GCHandle.Alloc(this);
    }

    public virtual void Dispose()
    {
        _gcHandle.Free();
        HookHelper.RemoveJumpHook(_functionAddress, _stolenByteCount, _trampolineAddress);
    }
}