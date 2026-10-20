using System.Net.WebSockets;
using SFSharp.Examples.DebugWeb.Transport;

namespace SF.DebugWeb.Tests;

public sealed class DebugWebSocketSenderTests
{
    [Fact]
    public async Task PacketBatchWaitsForWorldSendInsteadOfBeingDropped()
    {
        using BlockingSocket socket = new();
        DebugWebSocketSender sender = new(socket);
        Task world = sender.SendAsync([1], TestContext.Current.CancellationToken);
        await socket.FirstSendStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

        Task traffic = sender.SendAsync([2], TestContext.Current.CancellationToken);
        Assert.False(traffic.IsCompleted);
        socket.ReleaseFirstSend.SetResult();
        await Task.WhenAll(world, traffic).WaitAsync(TestContext.Current.CancellationToken);

        Assert.Equal(new byte[] { 1, 2 }, socket.Messages);
        Assert.Equal(1, socket.MaxConcurrentSends);
    }

    [Fact]
    public async Task CancelledWaitDoesNotReleaseAnotherSendsLock()
    {
        using BlockingSocket socket = new();
        DebugWebSocketSender sender = new(socket);
        Task first = sender.SendAsync([1], TestContext.Current.CancellationToken);
        await socket.FirstSendStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

        using CancellationTokenSource cancelled = new();
        Task waiting = sender.SendAsync([2], cancelled.Token);
        await cancelled.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);

        Task next = sender.SendAsync([3], TestContext.Current.CancellationToken);
        Assert.False(next.IsCompleted);
        socket.ReleaseFirstSend.SetResult();
        await Task.WhenAll(first, next).WaitAsync(TestContext.Current.CancellationToken);
        Assert.Equal(new byte[] { 1, 3 }, socket.Messages);
        Assert.Equal(1, socket.MaxConcurrentSends);
    }

    private sealed class BlockingSocket : WebSocket
    {
        public TaskCompletionSource FirstSendStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ReleaseFirstSend { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public List<byte> Messages { get; } = [];
        public int MaxConcurrentSends { get; private set; }
        private int _activeSends;

        public override WebSocketCloseStatus? CloseStatus => null;
        public override string? CloseStatusDescription => null;
        public override WebSocketState State => WebSocketState.Open;
        public override string? SubProtocol => null;
        public override void Abort() { }
        public override void Dispose() { }
        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken) => Task.CompletedTask;
        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken) => Task.CompletedTask;
        public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken) => throw new NotSupportedException();

        public override async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            int concurrent = Interlocked.Increment(ref _activeSends);
            MaxConcurrentSends = Math.Max(MaxConcurrentSends, concurrent);
            try
            {
                if (buffer.Array![buffer.Offset] == 1)
                {
                    FirstSendStarted.TrySetResult();
                    await ReleaseFirstSend.Task.WaitAsync(cancellationToken);
                }
                Messages.Add(buffer.Array![buffer.Offset]);
            }
            finally
            {
                Interlocked.Decrement(ref _activeSends);
            }
        }
    }
}
