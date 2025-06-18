namespace SFSharp;

public interface ISubHook<TArgs, TResult>
{
    TResult Process(TArgs args, Func<TArgs, TResult> next);
}

public interface ISubHook<TArgs>
{
    void Process(TArgs args, Action<TArgs> next);
}

public abstract class Hook<TArgs, TResult>
{
    private List<ISubHook<TArgs, TResult>> _subHooks = new();
    private readonly Func<TArgs, TResult> _baseFunction;
    private Func<TArgs, TResult> _invokeSubHooks;
    private bool _isProcessing = false;

    protected Hook(Func<TArgs, TResult> baseFunction)
    {
        _invokeSubHooks = _baseFunction = baseFunction;
    }

    public void AddSubHook(ISubHook<TArgs, TResult> subHook)
    {
        if (_isProcessing) _subHooks = _subHooks.ToList();

        _subHooks.Add(subHook);
        _invokeSubHooks = BuildHookChain();
    }
    public void RemoveSubHook(ISubHook<TArgs, TResult> subHook)
    {
        if (_isProcessing) _subHooks = _subHooks.ToList();

        _subHooks.Remove(subHook);
        _invokeSubHooks = BuildHookChain();
    }

    protected Func<TArgs, TResult> BuildHookChain()
    {
        var next = _baseFunction;
        foreach (var subHook in _subHooks.AsEnumerable().Reverse())
        {
            var current = next;
            next = args => subHook.Process(args, current);
        }
        return next;
    }

    protected TResult Process(TArgs args)
    {
        _isProcessing = true;
        var returnValue = _invokeSubHooks(args);
        _isProcessing = false;
        return returnValue;
    }
}

public abstract class Hook<TArgs>
{
    private List<ISubHook<TArgs>> _subHooks = new();
    private readonly Action<TArgs> _baseFunction;
    private Action<TArgs> _invokeSubHooks;
    private bool _isProcessing = false;

    protected Hook(Action<TArgs> baseFunction)
    {
        _invokeSubHooks = _baseFunction = baseFunction;
    }

    public void AddSubHook(ISubHook<TArgs> subHook)
    {
        if (_isProcessing) _subHooks = _subHooks.ToList();

        _subHooks.Add(subHook);
        _invokeSubHooks = BuildHookChain();
    }
    public void RemoveSubHook(ISubHook<TArgs> subHook)
    {
        if (_isProcessing) _subHooks = _subHooks.ToList();

        _subHooks.Remove(subHook);
        _invokeSubHooks = BuildHookChain();
    }

    protected Action<TArgs> BuildHookChain()
    {
        var next = _baseFunction;
        foreach (var subHook in _subHooks.AsEnumerable().Reverse())
        {
            var current = next;
            next = args => subHook.Process(args, current);
        }
        return next;
    }

    protected void Process(TArgs args)
    {
        _isProcessing = true;
        _invokeSubHooks(args);
        _isProcessing = false;
    }
}