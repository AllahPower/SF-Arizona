namespace SFSharp;

public interface ISubHook<TArgs>
{
    void Process(TArgs args, Action<TArgs> next);
}

public abstract class Hook<TArgs>
{
    private readonly List<ISubHook<TArgs>> _subHooks = new();
    private readonly Action<TArgs> _baseFunction;
    private Action<TArgs> _invokeSubHooks;
    private bool _isProcessing = false;

    protected Hook(Action<TArgs> baseFunction)
    {
        _baseFunction = baseFunction;
        _invokeSubHooks = _baseFunction;
    }

    public void AddSubHook(ISubHook<TArgs> subHook)
    {
        if (_isProcessing) throw new InvalidOperationException();
        _subHooks.Add(subHook);
        _invokeSubHooks = BuildHookChain();
    }
    public void RemoveSubHook(ISubHook<TArgs> subHook)
    {
        if (_isProcessing) throw new InvalidOperationException();
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
        return;
    }
}
