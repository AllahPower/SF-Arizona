namespace SFSharp.Abstractions.Game;

/// <summary>
/// Game start-up stages in the order they are reached. <c>Before*</c>/<c>After*</c> stages run inside the
/// GTA SA initialisation functions on the game thread; <see cref="NetGameReady"/> marks SA-MP's CNetGame.
/// </summary>
public enum SFGameLoadStage
{
    /// <summary>Not started yet.</summary>
    None = 0,

    /// <summary>First runtime tick: the first iteration of the GTA main loop, before any game data is loaded.</summary>
    Startup,

    /// <summary>Before <c>CGame::InitialiseCoreDataAfterRW</c>: handling.cfg, surface info, timecyc.dat, popcycle.dat, audio.</summary>
    BeforeCoreData,

    /// <summary>After <c>CGame::InitialiseCoreDataAfterRW</c>.</summary>
    AfterCoreData,

    /// <summary>Before <c>CGame::Init1</c>, which creates the GTA pools, the world and model info.</summary>
    BeforeInit1,

    /// <summary>After <c>CGame::Init1</c>. GTA pools exist; <c>DEFAULT.DAT</c> and <c>GTA.DAT</c> are loaded next.</summary>
    AfterInit1,

    /// <summary>Before <c>CGame::Init2</c>, after the level files are loaded.</summary>
    BeforeInit2,

    /// <summary>After <c>CGame::Init2</c>: streaming, paths, peds, animations and scripts are set up.</summary>
    AfterInit2,

    /// <summary>Before <c>CGame::Init3</c>: procedural interiors and real-time shadows.</summary>
    BeforeInit3,

    /// <summary>After <c>CGame::Init3</c>: the game world is initialised.</summary>
    AfterInit3,

    /// <summary>SA-MP CNetGame exists and the full <see cref="ISF"/> facade is available.</summary>
    NetGameReady,
}
