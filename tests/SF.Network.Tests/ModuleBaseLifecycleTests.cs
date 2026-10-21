using SFSharp.Abstractions.Modules;
using SFSharp.Runtime.Bootstrap;
using SFSharp.Runtime.Modules.Hosting;
using SFSharp.Runtime.Modules.Lifecycle;

namespace SF.Network.Tests;

public sealed class ModuleBaseLifecycleTests
{
    [Fact]
    public async Task FailedStartupStillStopsAndClearsInstanceState()
    {
        var module = new StartupFailure();
        ModuleDescriptor descriptor = ModuleDescriptorFactory.FromType(typeof(StartupFailure));
        using var context = new ModuleContext(descriptor, new ModuleRuntimeInfo(descriptor, false),
            CancellationToken.None, null!, new MainThreadDispatcher(new SFSynchronizationContext(_ => { })));
        await Assert.ThrowsAsync<InvalidOperationException>(() => module.RunAsync(context));
        Assert.Equal(["start", "fault", "stopping", "stopped"], module.Calls);
        Assert.Throws<InvalidOperationException>(() => module.CurrentContext);
    }

    [SFModule("test.startup-failure", "Startup failure")]
    private sealed class StartupFailure : SFModuleBase
    {
        public List<string> Calls { get; } = [];
        public IModuleContext CurrentContext => Context;
        protected override Task OnStartingAsync() { Calls.Add("start"); throw new InvalidOperationException("startup"); }
        protected override Task OnFaultedAsync(Exception exception) { Calls.Add("fault"); return Task.CompletedTask; }
        protected override Task OnStoppingAsync() { Calls.Add("stopping"); return Task.CompletedTask; }
        protected override Task OnStoppedAsync() { Calls.Add("stopped"); return Task.CompletedTask; }
        protected override Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
