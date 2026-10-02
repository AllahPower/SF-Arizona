namespace SFSharp.Abstractions.Game;

/// <summary>
/// Game start-up progress. Stages only move forward; a stage whose hook could not be installed, or that
/// passed before the runtime started, is skipped and its handlers are not called.
/// </summary>
public interface ISFGameLoading
{
    /// <summary>Latest stage reached.</summary>
    SFGameLoadStage Stage { get; }

    /// <summary>Raw GTA <c>gGameState</c> (0 start-up ... 9 playing).</summary>
    int GameState { get; }

    /// <summary>
    /// Calls <paramref name="handler"/> synchronously on the game thread when <paramref name="stage"/> is reached,
    /// inside the GTA initialisation function for <c>Before*</c>/<c>After*</c> stages. A handler for a stage that
    /// has already passed is never called. Dispose the result to unsubscribe.
    /// </summary>
    IDisposable Subscribe(SFGameLoadStage stage, Action handler);

    /// <summary>Completes once <paramref name="stage"/> is reached or skipped; already completed for past stages.</summary>
    Task WhenStageAsync(SFGameLoadStage stage);
}
