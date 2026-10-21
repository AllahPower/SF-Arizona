namespace SFSharp.Runtime.Game.Pools;

public sealed unsafe class SFGamePools(SFPlayers players, SFVehicles vehicles, ISFMainThread mainThread) : ISFGamePools
{
    public SFPlayers Players => players;
    public SFVehicles Vehicles => vehicles;
    public SFActors Actors { get; } = new(mainThread);
    public SFObjects Objects { get; } = new(mainThread);
    public SFPickups Pickups { get; } = new(mainThread);
    public SFMenus Menus { get; } = new(mainThread);
    public SFGangZones GangZones { get; } = new(mainThread);
    public SFLabels Labels { get; } = new(mainThread);
    public SFTextDraws TextDraws { get; } = new(mainThread);

    private bool TryGetInstance(out CNetGame* netGame)
    {
        RequireThread();
        return CNetGame.TryGetInstance(out netGame);
    }

    private bool TryGetPools(out CNetGamePools* pools)
    {
        RequireThread();
        return CNetGame.TryGetPools(out pools);
    }

    private void RequireThread()
    {
        if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP pools require the game thread.");
    }

    public bool IsAvailable => TryGetInstance(out _);
    public bool IsInitialized => TryGetPools(out _);
    public int RawState => TryGetInstance(out CNetGame* netGame) ? netGame->GetState() : 0;
    public SFSampGameState State
    {
        get
        {
            int raw = RawState;
            return SampEnvironment.GetOrDetect()?.Profile?.NormalizeState(raw) ?? SFSampGameState.Unknown;
        }
    }
    public bool IsLanMode => TryGetInstance(out CNetGame* netGame) && netGame->IsLanMode();
    public int InitializedPoolCount => TryGetPools(out CNetGamePools* pools) ? pools->GetInitializedCount() : 0;

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
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->InitializePools();
        }
    }

    public void UpdatePlayers()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->UpdatePlayers();
        }
    }

    public void ResetAll()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetPools();
        }
    }

    public void ResetPlayers()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetPlayerPool();
        }
    }

    public void ResetVehicles()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetVehiclePool();
        }
    }

    public void ResetActors()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetActorPool();
        }
    }

    public void ResetObjects()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetObjectPool();
        }
    }

    public void ResetPickups()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetPickupPool();
        }
    }

    public void ResetMenus()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetMenuPool();
        }
    }

    public void ResetGangZones()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetGangZonePool();
        }
    }

    public void ResetLabels()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetLabelPool();
        }
    }

    public void ResetTextDraws()
    {
        if (TryGetInstance(out CNetGame* netGame))
        {
            netGame->ResetTextDrawPool();
        }
    }
}
