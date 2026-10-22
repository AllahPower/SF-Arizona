using System.Runtime.CompilerServices;
using SFSharp.Runtime.Bootstrap;
using SFSharp.Runtime.Interop.Hooking;
using SFSharp.Runtime.Interop.Hooking.Hooks;
using SFSharp.Runtime.Samp;

namespace SF.Network.Tests;

public sealed class SampActionTests
{
    [Fact]
    public void SubscriptionsInstallOnceAndCanRestartWithoutRepatching()
    {
        MainThreadDispatcher mainThread = MainThread();
        Action<int>? publish = null;
        int installs = 0;
        var source = new SampActionEvent<int>("test", mainThread, callback =>
        {
            installs++;
            publish = callback;
            return new Binding();
        });
        Assert.Equal(0, installs);
        int first = 0, second = 0;

        IDisposable firstSubscription = source.Subscribe(value => first += value);
        publish!(2);
        firstSubscription.Dispose();
        firstSubscription.Dispose();
        using IDisposable secondSubscription = source.Subscribe(value => second += value);
        publish(3);

        Assert.Equal(1, installs);
        Assert.Equal(2, first);
        Assert.Equal(3, second);
    }

    [Fact]
    public void WorkerDeliveryIsDeferredAndDisposedSubscriberIsNotCalled()
    {
        MainThreadDispatcher mainThread = MainThread();
        Action<int>? publish = null;
        var source = new SampActionEvent<int>("test", mainThread, callback =>
        {
            publish = callback;
            return new Binding();
        });
        int received = 0, callbackThread = 0;
        using IDisposable subscription = source.Subscribe(value =>
        {
            received += value;
            callbackThread = Environment.CurrentManagedThreadId;
        });
        int disposedCalls = 0;
        IDisposable disposed = source.Subscribe(_ => disposedCalls++);

        RunWorker(() => publish!(7));
        Assert.Equal(0, received);
        disposed.Dispose();
        mainThread.Pump();

        Assert.Equal(7, received);
        Assert.Equal(Environment.CurrentManagedThreadId, callbackThread);
        Assert.Equal(0, disposedCalls);
    }

    [Fact]
    public void WorkerRegistrationFailsBeforeInstallingNativeHook()
    {
        MainThreadDispatcher mainThread = MainThread();
        int installs = 0;
        var source = new SampActionEvent<int>("test", mainThread, _ =>
        {
            installs++;
            return new Binding();
        });

        RunWorker(() => Assert.Throws<InvalidOperationException>(() => source.Subscribe(_ => { })));

        Assert.Equal(0, installs);
    }

    [Fact]
    public void FailedInstallationDoesNotRegisterHandlersOrRetryNativePatching()
    {
        int installs = 0;
        var source = new SampActionEvent<int>("test", MainThread(), _ =>
        {
            installs++;
            throw new InvalidOperationException("installation failed");
        });

        Assert.Throws<InvalidOperationException>(() => source.Subscribe(_ => { }));
        Assert.Throws<InvalidOperationException>(() => source.Subscribe(_ => { }));

        Assert.Equal(1, installs);
    }

    [Fact]
    public void ThrowingSubscriberDoesNotInterruptOtherSubscribers()
    {
        Action<int>? publish = null;
        var source = new SampActionEvent<int>("test", MainThread(), callback =>
        {
            publish = callback;
            return new Binding();
        });
        int received = 0;
        using IDisposable throwing = source.Subscribe(_ => throw new InvalidOperationException("subscriber failed"));
        using IDisposable next = source.Subscribe(value => received = value);

        publish!(42);

        Assert.Equal(42, received);
    }

    [Fact]
    public void DisposedSubscriptionReleasesItsHandlerEvenWhileSubscriptionIsRetained()
    {
        var source = new SampActionEvent<int>("test", MainThread(), _ => new Binding());
        (WeakReference target, IDisposable subscription) = SubscribeAndDispose(source);

        for (int i = 0; i < 10 && target.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        Assert.False(target.IsAlive);
        GC.KeepAlive(source);
        GC.KeepAlive(subscription);
    }

    [Fact]
    public void WarmMainThreadDispatchAndObserverAllocateNothing()
    {
        Action<int>? publish = null;
        var source = new SampActionEvent<int>("test", MainThread(), callback =>
        {
            publish = callback;
            return new Binding();
        });
        long sum = 0;
        using IDisposable subscription = source.Subscribe(value => sum += value);
        var hook = new TestHook();
        hook.AddSubHook(new SampActionObserver<int, int>((_, result) => publish!(result)));
        for (int i = 0; i < 1_000; i++) hook.Call(1);
        long before = GC.GetAllocatedBytesForCurrentThread();

        for (int i = 0; i < 10_000; i++) hook.Call(1);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0, allocated);
        Assert.Equal(11_000, sum);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(256)]
    [InlineData(-1)]
    public void CompletionPreservesRawResultAndRunsAfterOriginal(int result)
    {
        var hook = new TestHook();
        int observed = 0;
        hook.AddSubHook(new SampActionObserver<int, int>((args, value) =>
        {
            Assert.Equal(1, hook.Calls);
            Assert.Equal(result, args);
            observed = value;
        }));

        int actual = hook.Call(result);

        Assert.Equal(result, actual);
        Assert.Equal(result, observed);
        Assert.Equal(1, hook.Calls);
    }

    [Fact]
    public void ObserverFailureDoesNotRepeatOriginalOrLoseItsResult()
    {
        var hook = new TestHook();
        hook.AddSubHook(new SampActionObserver<int, int>((_, _) => throw new InvalidOperationException()));

        int actual = hook.Call(256);

        Assert.Equal(256, actual);
        Assert.Equal(1, hook.Calls);
    }

    [Fact]
    public void OriginalFailureDoesNotPublishCompletion()
    {
        var hook = new TestHook { Fail = true };
        int completed = 0;
        hook.AddSubHook(new SampActionObserver<int, int>((_, _) => completed++));

        Assert.Throws<InvalidOperationException>(() => hook.Call(1));

        Assert.Equal(1, hook.Calls);
        Assert.Equal(0, completed);
    }

    private static MainThreadDispatcher MainThread() => new(new SFSynchronizationContext(ex => throw ex));

    private static void RunWorker(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { failure = ex; }
        });
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(10)));
        Assert.Null(failure);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference, IDisposable) SubscribeAndDispose(SampActionEvent<int> source)
    {
        var target = new Subscriber();
        IDisposable subscription = source.Subscribe(target.OnAction);
        subscription.Dispose();
        return (new(target), subscription);
    }

    private sealed class Subscriber
    {
        public int Value;
        public void OnAction(int value) => Value = value;
    }

    private sealed class Binding : IDisposable
    {
        public void Dispose() { }
    }

    private sealed class TestHook : HookBase<int, int>
    {
        public int Calls;
        public bool Fail;
        public int Call(int value) => Process(value);
        protected override int InvokeOriginalFunction(int value)
        {
            Calls++;
            if (Fail) throw new InvalidOperationException("native routine failed");
            return value;
        }
    }
}
