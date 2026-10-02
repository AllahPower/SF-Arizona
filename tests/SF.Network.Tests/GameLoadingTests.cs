using SFSharp.Abstractions.Game;
using SFSharp.Runtime.Game.Loading;

namespace SF.Network.Tests;

public sealed class GameLoadingTests
{
    private readonly List<Exception> _reported = [];

    private SFGameLoading Create() => new(_reported.Add, static () => 7);

    [Fact]
    public void HandlersRunOnlyForTheirStageInSubscriptionOrder()
    {
        SFGameLoading loading = Create();
        List<string> calls = [];
        loading.Subscribe(SFGameLoadStage.BeforeInit1, () => calls.Add("a"));
        loading.Subscribe(SFGameLoadStage.BeforeInit1, () => calls.Add("b"));
        loading.Subscribe(SFGameLoadStage.AfterInit1, () => calls.Add("after"));

        loading.Reach(SFGameLoadStage.Startup);
        loading.Reach(SFGameLoadStage.BeforeInit1);

        Assert.Equal(["a", "b"], calls);
        Assert.Equal(SFGameLoadStage.BeforeInit1, loading.Stage);
        Assert.Equal(7, loading.GameState);
    }

    [Fact]
    public void SkippedStagesDropHandlersAndCompleteWaiters()
    {
        SFGameLoading loading = Create();
        bool called = false;
        loading.Subscribe(SFGameLoadStage.AfterCoreData, () => called = true);
        Task skipped = loading.WhenStageAsync(SFGameLoadStage.AfterCoreData);

        loading.Reach(SFGameLoadStage.NetGameReady);

        Assert.False(called);
        Assert.True(skipped.IsCompleted);
    }

    [Fact]
    public void PastStagesAreCompletedAndNeverCallNewHandlers()
    {
        SFGameLoading loading = Create();
        loading.Reach(SFGameLoadStage.AfterInit2);
        bool called = false;

        using IDisposable subscription = loading.Subscribe(SFGameLoadStage.BeforeInit2, () => called = true);
        loading.Reach(SFGameLoadStage.BeforeInit2);

        Assert.False(called);
        Assert.True(loading.WhenStageAsync(SFGameLoadStage.Startup).IsCompleted);
        Assert.Equal(SFGameLoadStage.AfterInit2, loading.Stage);
    }

    [Fact]
    public void FailingHandlerIsReportedAndDoesNotStopTheOthers()
    {
        SFGameLoading loading = Create();
        bool secondCalled = false;
        loading.Subscribe(SFGameLoadStage.Startup, () => throw new InvalidOperationException("boom"));
        loading.Subscribe(SFGameLoadStage.Startup, () => secondCalled = true);

        loading.Reach(SFGameLoadStage.Startup);

        Assert.True(secondCalled);
        Assert.IsType<InvalidOperationException>(Assert.Single(_reported));
    }

    [Fact]
    public void DisposedSubscriptionIsNotCalled()
    {
        SFGameLoading loading = Create();
        bool called = false;
        loading.Subscribe(SFGameLoadStage.Startup, () => called = true).Dispose();

        loading.Reach(SFGameLoadStage.Startup);

        Assert.False(called);
    }
}
