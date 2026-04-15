namespace SFSharp;

public sealed unsafe class SFGamePools : ISFGamePools
{
    public SFPlayers Players => SF.Players;
    public SFVehicles Vehicles => SF.Vehicles;
    public SFActors Actors { get; } = new();
    public SFObjects Objects { get; } = new();
    public SFPickups Pickups { get; } = new();
    public SFMenus Menus { get; } = new();
    public SFGangZones GangZones { get; } = new();
    public SFLabels Labels { get; } = new();
    public SFTextDraws TextDraws { get; } = new();

    public bool IsAvailable => CNetGame.TryGetInstance(out _);
    public bool IsInitialized => CNetGame.TryGetPools(out _);
    public int State => CNetGame.TryGetInstance(out CNetGame* netGame) ? netGame->GetState() : 0;
    public bool IsLanMode => CNetGame.TryGetInstance(out CNetGame* netGame) && netGame->IsLanMode();
    public int InitializedPoolCount => CNetGame.TryGetPools(out CNetGamePools* pools) ? pools->GetInitializedCount() : 0;

    ISFPlayers ISFGamePools.Players => Players;
    ISFVehicles ISFGamePools.Vehicles => Vehicles;
    ISFActors ISFGamePools.Actors => Actors;
    ISFObjects ISFGamePools.Objects => Objects;
    ISFPickups ISFGamePools.Pickups => Pickups;
    ISFMenus ISFGamePools.Menus => Menus;
    ISFGangZones ISFGamePools.GangZones => GangZones;
    ISFLabels ISFGamePools.Labels => Labels;
    ISFTextDraws ISFGamePools.TextDraws => TextDraws;

    public void InitializePools()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->InitializePools();
        }
    }

    public void UpdatePlayers()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->UpdatePlayers();
        }
    }

    public void ResetAll()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetPools();
        }
    }

    public void ResetPlayers()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetPlayerPool();
        }
    }

    public void ResetVehicles()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetVehiclePool();
        }
    }

    public void ResetActors()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetActorPool();
        }
    }

    public void ResetObjects()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetObjectPool();
        }
    }

    public void ResetPickups()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetPickupPool();
        }
    }

    public void ResetMenus()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetMenuPool();
        }
    }

    public void ResetGangZones()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetGangZonePool();
        }
    }

    public void ResetLabels()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetLabelPool();
        }
    }

    public void ResetTextDraws()
    {
        if (CNetGame.TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetTextDrawPool();
        }
    }
}
