
namespace SFSharp.Runtime.Modules;

[SFModule("nod-shaker", "NodShaker", Category = "Utility", Description = "Maps extra keys to +/- chat shortcuts.", ExecutionModel = ModuleExecutionModel.MainThread, Order = 30)]
public class NodShaker : SFModuleBase
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        ISF sf = Context.SF;
        while (!cancellationToken.IsCancellationRequested)
        {
            using (IDisposable _ = Context.TrackLoop("keyboard-poll"))
            {
                if (sf.Keyboard.IsKeyPressed((byte)VK.ADD))
                {
                    sf.Chat.Send("+");
                    Context.IncrementCounter("plus.sent");
                }

                if (sf.Keyboard.IsKeyPressed((byte)VK.SUBTRACT))
                {
                    sf.Chat.Send("-");
                    Context.IncrementCounter("minus.sent");
                }

                Context.Heartbeat("watching +/- keys");
            }

            await Task.Yield();
        }
    }
}
