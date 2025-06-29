using SFSharp;

public class NodShaker : ISFSharpModule
{
    public async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (SF.Keyboard.IsKeyPressed(VK.ADD))
            {
                SF.Chat.Send("+");
            }
            if (SF.Keyboard.IsKeyPressed(VK.SUBTRACT))
            {
                SF.Chat.Send("-");
            }
            await Task.Yield();
        }
    }
}
