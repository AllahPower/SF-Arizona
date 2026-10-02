using System.Diagnostics;
using Microsoft.Extensions.Logging;
using SFSharp.Abstractions.Game;
using SFSharp.Abstractions.Modules;

namespace SFSharp.Examples.EarlyLoading;

/// <summary>Logs every game load stage it still sees, with the raw gGameState and the time since the plugin loaded.</summary>
public sealed class LoadStageLogger : ISFEarlyModule
{
    private readonly Stopwatch _sinceLoad = Stopwatch.StartNew();

    public void OnGameLoading(ISFEarlyContext context)
    {
        context.Log.LogInformation("Plugin loaded at stage {Stage}, gGameState={GameState}", context.Loading.Stage, context.Loading.GameState);

        foreach (SFGameLoadStage stage in Enum.GetValues<SFGameLoadStage>())
        {
            if (stage > context.Loading.Stage)
            {
                context.Loading.Subscribe(stage, () => context.Log.LogInformation(
                    "{Stage} after {Elapsed} ms, gGameState={GameState}",
                    stage,
                    _sinceLoad.ElapsedMilliseconds,
                    context.Loading.GameState));
            }
        }
    }
}
