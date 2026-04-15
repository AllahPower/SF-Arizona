namespace SFSharp;

/// <summary>Read-only game pool facade.</summary>
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
