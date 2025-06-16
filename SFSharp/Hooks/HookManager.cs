using System.Drawing;

namespace SFSharp;

public static class HookManager {

}

public abstract class Hook<TArgs> {
    private readonly List<ISubHook<TArgs>> _subHooks = new();
    private bool _isProcessing = false;
    public void AddSubHook(ISubHook<TArgs> subHook) {
        if (_isProcessing) throw new InvalidOperationException();
        _subHooks.Add(subHook);
    }
    public void RemoveSubHook(ISubHook<TArgs> subHook) {
        if (_isProcessing) throw new InvalidOperationException();
        _subHooks.Remove(subHook);
    }

    protected void Process(TArgs args) {
        _isProcessing = true;

    }
}

public interface ISubHook<TArgs> {
    void Process(TArgs args, Action<TArgs> next);
}

public interface ISubHook<TArgs, TResult> {
    TResult Process(TArgs args, Func<TArgs, TResult> next);
}