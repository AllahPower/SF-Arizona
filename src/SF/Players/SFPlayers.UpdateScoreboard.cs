
namespace SFSharp.Runtime.Game;

public partial class SFPlayers : ISubHook<UpdateScoresPingsIpsArgs, NoRetValue>
{
    public event Action? ScoreboardUpdated;

    public Task UpdateScoreboard()
    {
        _tcs ??= new(TaskCreationOptions.RunContinuationsAsynchronously);
        CNetGame.Instance.UpdatePlayers();
        return _tcs.Task;
    }

    private TaskCompletionSource? _tcs;

    public NoRetValue Process(UpdateScoresPingsIpsArgs args, Func<UpdateScoresPingsIpsArgs, NoRetValue> next)
    {
        next(args);
        ScoreboardUpdated?.Invoke();
        _tcs?.SetResult();
        _tcs = null;
        return default;
    }
}
