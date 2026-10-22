using SFSharp.Runtime.Bootstrap;
using SFSharp.Runtime.Diagnostics;
using SFSharp.Runtime.Game.Players;
using SFSharp.Runtime.Game.Pools;
using SFSharp.Runtime.Samp;
using SFSharp.Runtime.Ui;

namespace SF.Network.Tests;

public sealed class SampThreadSafetyTests
{
    [Fact]
    public void NativeFacadesRejectWorkerAccessBeforeResolvingGameModules()
    {
        var mainThread = new MainThreadDispatcher(new SFSynchronizationContext(_ => { }));
        var players = new SFPlayers(mainThread);
        var vehicles = new SFVehicles(mainThread);
        var pools = new SFGamePools(players, vehicles, mainThread);
        var bridge = new SFSampEntities(players, vehicles, pools, mainThread);
        var camera = new SFSampCamera(players, null!, mainThread);
        var exceptions = new ExceptionReporter();
        var chat = new SFChat(exceptions, mainThread);
        var dialog = new SFDialog(mainThread, exceptions);
        var actions = new SFSampActions(mainThread, new SFSharp.Runtime.Gta.SFGameClock(), bridge);
        Exception? failure = null;

        var worker = new Thread(() =>
        {
            try
            {
                Assert.Throws<InvalidOperationException>(() => players.GetScore(0));
                Assert.Throws<InvalidOperationException>(() => players.EnumeratePlayers());
                Assert.Throws<InvalidOperationException>(() => vehicles.Exists(0));
                Assert.Throws<InvalidOperationException>(() => vehicles.EnumerateIds());
                Assert.Throws<InvalidOperationException>(() => pools.ResetAll());
                Assert.Throws<InvalidOperationException>(() => pools.IsAvailable);
                Assert.Throws<InvalidOperationException>(() => pools.Actors.Exists(0));
                Assert.Throws<InvalidOperationException>(() => pools.Objects.Exists(0));
                Assert.Throws<InvalidOperationException>(() => pools.Pickups.Exists(0));
                Assert.Throws<InvalidOperationException>(() => pools.Menus.Exists(0));
                Assert.Throws<InvalidOperationException>(() => pools.GangZones.Exists(0));
                Assert.Throws<InvalidOperationException>(() => pools.Labels.Exists(0));
                Assert.Throws<InvalidOperationException>(() => pools.TextDraws.Exists(0));
                Assert.Throws<InvalidOperationException>(() => bridge.TryGetLocalPlayer(out _));
                Assert.Throws<InvalidOperationException>(() => bridge.TryGetPlayerId(default, out _));
                Assert.Throws<InvalidOperationException>(() => bridge.OnSampEntityChanged(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.OnSpawnRequested(() => { }));
                Assert.Throws<InvalidOperationException>(() => actions.OnSpawnCompleted(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.OnDeathNotified(() => { }));
                Assert.Throws<InvalidOperationException>(() => actions.OnClassRequested(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.OnInteriorChanged(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.OnVehicleEntryNotified(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.OnVehicleExitNotified(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.OnRestarted(() => { }));
                Assert.Throws<InvalidOperationException>(() => actions.Session.OnConnectionEvent(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.Session.OnStateChanged(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.Session.OnConnectAttempt(() => { }));
                Assert.Throws<InvalidOperationException>(() => actions.RemotePlayers.OnSyncProcessed(SFSharp.Abstractions.Samp.Events.SFSampSyncKind.OnFoot, _ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.RemotePlayers.OnStateChanged(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.RemotePlayers.OnActionCompleted(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.Pools.OnChanged(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.TextDraws.OnSelectionChanged(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.TextDraws.OnHoveredChanged(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.TextDraws.OnSelectionNotified(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.Pickups.OnChanged(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.Pickups.OnCollectionNotified(_ => { }));
                Assert.Throws<InvalidOperationException>(() => actions.Spectating.OnChanged(_ => { }));
                Assert.Throws<InvalidOperationException>(() => camera.AimPosition);
                Assert.Throws<InvalidOperationException>(() => camera.Target);
                Assert.Throws<InvalidOperationException>(() => chat.Add("test"));
                Assert.Throws<InvalidOperationException>(() => chat.Send("test"));
                Assert.Throws<InvalidOperationException>(() => { _ = dialog.Show(default, "", "", "", ""); });
            }
            catch (Exception ex) { failure = ex; }
        });
        worker.Start();
        Assert.True(worker.Join(TimeSpan.FromSeconds(10)));
        Assert.Null(failure);
    }
}
