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

public abstract unsafe class JumpHook<TArgs, TResult> : HookBase<TArgs, TResult>, IDisposable
{
    protected abstract void* InjectedFunction { get; }
    protected void* OriginalFunction => (void*)_trampolineAddress;

    private readonly uint _stolenByteCount;
    private readonly uint _functionAddress;
    private readonly uint _trampolineAddress;
    private readonly GCHandle _gcHandle;

    protected JumpHook(uint stolenByteCount, uint functionAddress)
    {
        _stolenByteCount = stolenByteCount;
        _functionAddress = functionAddress;

        _trampolineAddress = HookHelper.InstallJumpHook(
            _functionAddress,
            _stolenByteCount,
            (uint)InjectedFunction
        );

        _gcHandle = GCHandle.Alloc(this);
    }

    public virtual void Dispose()
    {
        _gcHandle.Free();
        HookHelper.RemoveJumpHook(_functionAddress, _stolenByteCount, _trampolineAddress);
    }
}

public abstract unsafe class CallHook<TArgs, TResult> : HookBase<TArgs, TResult>, IDisposable
{
    protected abstract void* InjectedFunction { get; }
    protected void* OriginalFunction => (void*)_callTargetAddress;

    private readonly uint _stolenByteCount;
    private readonly uint _callSiteAddress;
    private readonly uint _callTargetAddress; // The base class doesn't really need this, but it allows for uniform API with JumpHook
    private readonly uint _originalByteBuffer;
    private readonly GCHandle _gcHandle;

    public CallHook(uint stolenByteCount, uint callSiteAddress, uint callTargetAddress)
    {
        _callSiteAddress = callSiteAddress;
        _callTargetAddress = callTargetAddress;
        _stolenByteCount = stolenByteCount;

        _originalByteBuffer = HookHelper.InstallCallHook(
            _callSiteAddress,
            _stolenByteCount,
            (uint)InjectedFunction
        );

        _gcHandle = GCHandle.Alloc(this);
    }

    public virtual void Dispose()
    {
        _gcHandle.Free();
        HookHelper.RemoveCallHook(_callSiteAddress, _stolenByteCount, _originalByteBuffer);
    }
}