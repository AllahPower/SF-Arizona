using System.Runtime.CompilerServices;
using SFSharp.Runtime.Ui;

namespace SF.Network.Tests;

public sealed class ChatCommandRegistryTests
{
    [Fact]
    public void OldRegistrationCannotUnregisterReplacement()
    {
        var commands = new ChatCommandRegistry();
        var old = commands.Register("test", _ => { });
        old.Dispose();
        bool invoked = false;
        using var current = commands.Register("test", _ => invoked = true);

        old.Dispose();

        Assert.True(commands.TryInvoke("test", null));
        Assert.True(invoked);
    }

    [Fact]
    public void ConcurrentRegistrationLookupAndDisposalAreSafe()
    {
        var commands = new ChatCommandRegistry();
        Parallel.For(0, 2000, id =>
        {
            string name = $"command{id}";
            using var registration = commands.Register(name, value => Assert.Equal(name, value));
            Assert.True(commands.TryInvoke(name, name));
        });
        Assert.False(commands.TryInvoke("command0", null));
    }

    [Fact]
    public void DisposedRegistrationDoesNotRetainHandlerOwner()
    {
        var commands = new ChatCommandRegistry();
        var (registration, owner) = Register(commands);
        registration.Dispose();
        for (int i = 0; i < 10 && owner.IsAlive; i++) { GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect(); }

        Assert.False(owner.IsAlive);
        GC.KeepAlive(registration);
        GC.KeepAlive(commands);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (IDisposable, WeakReference) Register(ChatCommandRegistry commands)
    {
        var owner = new Owner();
        return (commands.Register("test", owner.Call), new(owner));
    }

    private sealed class Owner { public void Call(string? args) { } }
}
