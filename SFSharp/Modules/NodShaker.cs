using SFSharp;

public class NodShaker : ISFSharpModule
{
    public async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (SF.IsKeyPressed(VK.ADD))
            {
                SF.SendChatMessage("+");
            }
            if (SF.IsKeyPressed(VK.SUBTRACT))
            {
                SF.SendChatMessage("-");
            }
            await Task.Yield();
        }
    }
}
