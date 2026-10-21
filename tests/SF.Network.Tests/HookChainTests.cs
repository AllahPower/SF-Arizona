using SFSharp.Runtime.Interop.Hooking;
using SFSharp.Runtime.Interop.Hooking.Hooks;
using System.Runtime.InteropServices;

namespace SF.Network.Tests;

public sealed class HookChainTests
{
    [Fact]
    public void FailureAfterNextDoesNotRepeatOriginal()
    {
        var hook = new TestHook();
        hook.AddSubHook(new Handler((value, next) => { next(value + 1); throw new InvalidOperationException(); }));

        Assert.Equal(3, hook.Call(2));
        Assert.Equal(1, hook.Calls);
    }

    [Fact]
    public void FailureBeforeNextCallsOriginalOnce()
    {
        var hook = new TestHook();
        hook.AddSubHook(new Handler((_, _) => throw new InvalidOperationException()));

        Assert.Equal(2, hook.Call(2));
        Assert.Equal(1, hook.Calls);
    }

    [Fact]
    public void RepeatedNextCallsDoNotRepeatOriginal()
    {
        var hook = new TestHook();
        hook.AddSubHook(new Handler((value, next) => next(value) + next(value)));

        Assert.Equal(4, hook.Call(2));
        Assert.Equal(1, hook.Calls);
    }

    [Fact]
    public void SuppressionDoesNotCallOriginal()
    {
        var hook = new TestHook();
        hook.AddSubHook(new Handler((_, _) => 17));

        Assert.Equal(17, hook.Call(2));
        Assert.Equal(0, hook.Calls);
    }

    [Fact]
    public void FailureAfterSuppressedNextPreservesSuppression()
    {
        var hook = new TestHook();
        hook.AddSubHook(new Handler((value, next) => { next(value); throw new InvalidOperationException(); }));
        hook.AddSubHook(new Handler((_, _) => 17));

        Assert.Equal(17, hook.Call(2));
        Assert.Equal(0, hook.Calls);
    }

    [Fact]
    public void ReentrantInvocationHasIndependentOriginalState()
    {
        var hook = new TestHook();
        hook.AddSubHook(new Handler((value, next) => value == 1 ? hook.Call(2) + next(value) : next(value)));

        Assert.Equal(3, hook.Call(1));
        Assert.Equal(2, hook.Calls);
    }

    [Fact]
    public void RegistrationDuringInvocationChangesOnlyNextSnapshot()
    {
        var hook = new TestHook();
        var suppress = new Handler((_, _) => 17);
        hook.AddSubHook(new Handler((value, next) => { hook.AddSubHook(suppress); return next(value); }));

        Assert.Equal(2, hook.Call(2));
        Assert.Equal(17, hook.Call(2));
    }

    [Fact]
    public void OriginalFailureIsNotRetried()
    {
        var hook = new TestHook { FailOriginal = true };

        Assert.Throws<InvalidOperationException>(() => hook.Call(2));
        Assert.Equal(1, hook.Calls);
    }

    [Fact]
    public void NativeBoundaryContainsOriginalFailureWithoutRetry()
    {
        var hook = new BoundaryHook();

        Assert.False(hook.Call());
        Assert.Equal(1, hook.Calls);
    }

    [Fact]
    public void LoadingDelegatesHaveDistinctVerifiedCdeclSignatures()
    {
        var core = typeof(GameCoreDataHook.Native).GetMethod("Invoke")!;
        var init = typeof(GameInitStageHook.Native).GetMethod("Invoke")!;

        Assert.Empty(core.GetParameters());
        Assert.Equal(typeof(nint), Assert.Single(init.GetParameters()).ParameterType);
        Assert.Equal(typeof(byte), core.ReturnType);
        Assert.Equal(typeof(byte), init.ReturnType);
        Assert.Equal(CallingConvention.Cdecl,
            typeof(GameCoreDataHook.Native).GetCustomAttributes(typeof(UnmanagedFunctionPointerAttribute), false)
                .Cast<UnmanagedFunctionPointerAttribute>().Single().CallingConvention);
    }

    [Fact]
    public void ConcurrentRegistrationAndInvocationAreSafe()
    {
        var hook = new TestHook();
        Parallel.For(0, 2000, value =>
        {
            var handler = new Handler((arg, next) => next(arg));
            hook.AddSubHook(handler);
            Assert.Equal(value, hook.Call(value));
            hook.RemoveSubHook(handler);
        });

        Assert.Equal(2000, hook.Calls);
    }

    [Fact]
    public void WarmHookChainDoesNotAllocatePerCall()
    {
        var hook = new TestHook();
        hook.AddSubHook(new Handler((value, next) => next(value)));
        for (int i = 0; i < 1000; i++) hook.Call(i);

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10000; i++) hook.Call(i);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
    }

    private sealed class TestHook : HookBase<int, int>
    {
        public int Calls;
        public bool FailOriginal;
        public int Call(int value) => Process(value);
        protected override int InvokeOriginalFunction(int value)
        {
            Interlocked.Increment(ref Calls);
            if (FailOriginal) throw new InvalidOperationException("original failed");
            return value;
        }
    }

    private sealed class Handler(Func<int, Func<int, int>, int> process) : ISubHook<int, int>
    {
        public int Process(int args, Func<int, int> next) => process(args, next);
    }

    private sealed class BoundaryHook : NativeHook<int, bool, Action>
    {
        public int Calls;
        public bool Call() => ProcessNative(0, false);
        protected override bool InvokeOriginalFunction(int args)
        {
            Calls++;
            throw new InvalidOperationException("native boundary test");
        }
    }
}
