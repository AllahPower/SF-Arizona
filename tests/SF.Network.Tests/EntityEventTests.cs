using SFSharp.Abstractions.Game;
using SFSharp.Runtime.Game.Entities;
using SFSharp.Runtime.Interop.Hooking.Hooks;

namespace SF.Network.Tests;

public sealed class EntityEventTests
{
    [Fact]
    public void EventSourceCallsHandlersInSubscriptionOrderUntilDisposed()
    {
        EntityEventSource<int> source = new("test");
        List<string> calls = [];
        IDisposable first = source.Subscribe(value => calls.Add($"a{value}"));
        using IDisposable second = source.Subscribe(value => calls.Add($"b{value}"));

        source.Raise(1);
        first.Dispose();
        first.Dispose();
        source.Raise(2);

        Assert.Equal(["a1", "b1", "b2"], calls);
        Assert.True(source.HasHandlers);
    }

    [Fact]
    public void EventSourceReportsNoHandlersAfterLastUnsubscribe()
    {
        EntityEventSource<int> source = new("test");
        source.Subscribe(_ => { }).Dispose();

        Assert.False(source.HasHandlers);
    }

    [Theory]
    [InlineData(SFSampEntityChange.Created, true, "original,event")]
    [InlineData(SFSampEntityChange.Created, false, "original")]
    [InlineData(SFSampEntityChange.StreamedIn, true, "original,event")]
    [InlineData(SFSampEntityChange.Deleted, true, "event,original")]
    [InlineData(SFSampEntityChange.StreamedOut, false, "event,original")]
    public void LifecycleEventsWrapTheOriginalCall(SFSampEntityChange change, bool originalSucceeds, string expectedOrder)
    {
        List<string> calls = [];
        SampEntityLifecycleSubHook<FakeLifecycleArgs> subHook = new((_, raised) =>
        {
            Assert.Equal(change, raised);
            calls.Add("event");
        });

        bool result = subHook.Process(new FakeLifecycleArgs(change), _ =>
        {
            calls.Add("original");
            return originalSucceeds;
        });

        Assert.Equal(originalSucceeds, result);
        Assert.Equal(expectedOrder, string.Join(',', calls));
    }

    private readonly record struct FakeLifecycleArgs(SFSampEntityChange Change) : ISampEntityLifecycleArgs
    {
        public SFSampEntityType Type => SFSampEntityType.Vehicle;
        public ushort SampId => 7;
    }
}
