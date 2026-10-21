using SFSharp.Abstractions.Modules;
using SFSharp.Runtime.Bootstrap;
using SFSharp.Runtime.Modules.Hosting;
using SFSharp.Runtime.Modules.Lifecycle;

namespace SF.Network.Tests;

public sealed class ModuleContextLifetimeTests
{
    [Fact]
    public void LateWorkerRegistrationReleasesOnGameThread()
    {
        var mainThread = new MainThreadDispatcher(new SFSynchronizationContext(_ => { }));
        var descriptor = ModuleDescriptorFactory.FromType(typeof(FixtureModule));
        var runtime = new ModuleRuntimeInfo(descriptor, false);
        var context = new ModuleContext(descriptor, runtime, CancellationToken.None, null!, mainThread);
        context.Dispose();
        int ownerThread = Environment.CurrentManagedThreadId;
        int releasedOn = 0;
        Exception? failure = null;
        var worker = new Thread(() =>
        {
            try
            {
                context.RegisterDisposable(new Resource(() => releasedOn = Environment.CurrentManagedThreadId));
            }
            catch (Exception ex) { failure = ex; }
        });

        worker.Start();
        long deadline = Environment.TickCount64 + 5000;
        while (worker.IsAlive && Environment.TickCount64 < deadline)
        {
            mainThread.Pump();
            Thread.Yield();
        }

        Assert.True(worker.Join(TimeSpan.FromSeconds(1)));
        Assert.IsType<ObjectDisposedException>(failure);
        Assert.Equal(ownerThread, releasedOn);
    }

    [SFModule("fixture.lifetime", "Lifetime fixture")]
    private sealed class FixtureModule : SFModuleBase
    {
        protected override Task ExecuteAsync(CancellationToken token) => Task.CompletedTask;
    }

    private sealed class Resource(Action dispose) : IDisposable { public void Dispose() => dispose(); }
}
