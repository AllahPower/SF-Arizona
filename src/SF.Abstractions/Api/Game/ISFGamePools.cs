namespace SFSharp.Abstractions.Game;

/// <summary>Read-only game pool facade.</summary>
/// <remarks>
/// NOT thread-safe. The sub-facades (<see cref="Actors"/>, <see cref="Objects"/>, etc.) all read
/// native SA-MP pool memory and must be queried on the main game thread.
/// </remarks>
public interface ISFGamePools
{
    ISFPlayers Players { get; }
    ISFVehicles Vehicles { get; }
    ISFActors Actors { get; }
    ISFObjects Objects { get; }
    ISFPickups Pickups { get; }
    ISFMenus Menus { get; }
    ISFGangZones GangZones { get; }
    ISFLabels Labels { get; }
    ISFTextDraws TextDraws { get; }

    bool IsAvailable { get; }
    bool IsInitialized { get; }
    int State { get; }
    bool IsLanMode { get; }
    int InitializedPoolCount { get; }
}
