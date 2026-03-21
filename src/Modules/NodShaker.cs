using SFSharp;

[SFModule("nod-shaker", "NodShaker", Category = "Utility", Description = "Maps extra keys to +/- chat shortcuts.", ExecutionModel = ModuleExecutionModel.MainThread, Order = 30)]
public class NodShaker : SFModuleBase
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using (ModuleLoopScope _ = Context.TrackLoop("keyboard-poll"))
            {
                if (SF.Keyboard.IsKeyPressed(VK.ADD))
                {
                    SF.Chat.Send("+");
                    Context.IncrementCounter("plus.sent");
                }

                if (SF.Keyboard.IsKeyPressed(VK.SUBTRACT))
                {
                    SF.Chat.Send("-");
                    Context.IncrementCounter("minus.sent");
                }

                Context.Heartbeat("watching +/- keys");
            }

            await Task.Yield();
        }
    }
}
