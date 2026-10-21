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
