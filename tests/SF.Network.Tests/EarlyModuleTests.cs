using SFSharp.Abstractions.Game;
using SFSharp.Abstractions.Modules;
using SFSharp.Runtime.Game.Loading;
using SFSharp.Runtime.Modules.PluginLoading;

namespace SF.Network.Tests;

public sealed class EarlyModuleTests
{
    private readonly SFGameLoading _loading = new(_ => { }, static () => 0);

    [Fact]
    public void EarlyModuleSubscriptionsEndWhenTheHostIsDisposed()
    {
        RecordingEarlyModule.Calls.Clear();
        EarlyModuleHost host = EarlyModuleHost.Start("test", [typeof(RecordingEarlyModule)], _loading);

        _loading.Reach(SFGameLoadStage.Startup);
        host.Dispose();
        _loading.Reach(SFGameLoadStage.BeforeInit1);

        Assert.Equal(["loading:None", "Startup", "disposed"], RecordingEarlyModule.Calls);
    }

    [Fact]
    public void FailingEarlyModuleDisposesTheOnesAlreadyStarted()
    {
        RecordingEarlyModule.Calls.Clear();

        Assert.Throws<InvalidOperationException>(() =>
            EarlyModuleHost.Start("test", [typeof(RecordingEarlyModule), typeof(ThrowingEarlyModule)], _loading));

        _loading.Reach(SFGameLoadStage.Startup);
        Assert.Equal(["loading:None", "disposed"], RecordingEarlyModule.Calls);
    }

    [Fact]
    public void FindTypesReturnsOnlyConcreteEarlyModulesFromTheAssembly()
    {
        Type[] found = EarlyModuleHost.FindTypes(
            [typeof(RecordingEarlyModule), typeof(ISFEarlyModule), typeof(string)],
            typeof(RecordingEarlyModule).Assembly);

        Assert.Equal([typeof(RecordingEarlyModule)], found);
    }

    public sealed class RecordingEarlyModule : ISFEarlyModule, IDisposable
    {
        public static List<string> Calls { get; } = [];

        public void OnGameLoading(ISFEarlyContext context)
        {
            Calls.Add($"loading:{context.Loading.Stage}");
            context.Loading.Subscribe(SFGameLoadStage.Startup, () => Calls.Add("Startup"));
            context.Loading.Subscribe(SFGameLoadStage.BeforeInit1, () => Calls.Add("BeforeInit1"));
        }

        public void Dispose() => Calls.Add("disposed");
    }

    public sealed class ThrowingEarlyModule : ISFEarlyModule
    {
        public void OnGameLoading(ISFEarlyContext context) => throw new InvalidOperationException("boom");
    }
}
