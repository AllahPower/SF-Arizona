
namespace SFSharp;

public partial class SFPlayers : ISubHook<UpdateScoresPingsIpsArgs, NoRetValue>
{
    public Task UpdateScoreboard()
    {
        CNetGame.Instance.UpdatePlayers();
        _tcs ??= new(TaskCreationOptions.RunContinuationsAsynchronously);
        return _tcs.Task;
    }

    private TaskCompletionSource? _tcs;

    public NoRetValue Process(UpdateScoresPingsIpsArgs args, Func<UpdateScoresPingsIpsArgs, NoRetValue> next)
    {
        next(args);
        _tcs?.SetResult();
        _tcs = null;
        return default;
    }
}