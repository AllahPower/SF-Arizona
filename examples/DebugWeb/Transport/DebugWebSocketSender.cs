using System.Net.WebSockets;

namespace SFSharp.Examples.DebugWeb.Transport;

internal sealed class DebugWebSocketSender(WebSocket socket)
{
    private readonly SemaphoreSlim _sendLock = new(1, 1);

    public async Task SendAsync(byte[] json, CancellationToken cancellationToken)
    {
        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            await socket.SendAsync(json, WebSocketMessageType.Text, true, cancellationToken);
        }
        finally
        {
            _sendLock.Release();
        }
    }
}
