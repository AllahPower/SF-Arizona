using SFSharp.Runtime.Modules.Hosting;

namespace SF.Network.Tests;

public sealed class ModuleAsyncCommandTests
{
    [Fact]
    public async Task SynchronousPrefixRunsOnCallerAndOverlapIsIgnored()
    {
        var scope = new ModuleRunScope(CancellationToken.None, (_, _) => { }, _ => { });
        int owner = Environment.CurrentManagedThreadId;
        int calls = 0;
        var command = new ModuleAsyncCommand(scope, async (args, token) =>
        {
            Assert.Equal(owner, Environment.CurrentManagedThreadId);
            Assert.Equal("first", args);
            calls++;
            await Task.Delay(Timeout.InfiniteTimeSpan, token);
        });

        command.Invoke("first");
        command.Invoke("second");
        await scope.DrainAsync();
        scope.Dispose();

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task ReentrantInvocationDoesNotStartAnotherHandler()
    {
        using var scope = new ModuleRunScope(CancellationToken.None, (_, _) => { }, _ => { });
        int calls = 0;
        ModuleAsyncCommand command = null!;
        command = new(scope, (_, _) =>
        {
            calls++;
            command.Invoke(null);
            return Task.CompletedTask;
        });

        command.Invoke(null);
        command.Invoke(null);
        await scope.DrainAsync();

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task HandlerFaultBecomesRunFailure()
    {
        using var scope = new ModuleRunScope(CancellationToken.None, (_, _) => { }, _ => { });
        var command = new ModuleAsyncCommand(scope, (_, _) => throw new InvalidOperationException("command failed"));

        command.Invoke(null);

        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => scope.DrainAsync());
        Assert.Equal("command failed", failure.Message);
    }
}
