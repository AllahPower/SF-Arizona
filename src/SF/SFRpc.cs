using SFSharp.Interop.RakNet.Packets.Enum;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SFSharp;

public sealed class SFRpc
{
    public RpcHandlerManager Handlers => SFBootstrap.RpcHandlers;
    public OutgoingRpcManager OutgoingHandlers => SFBootstrap.OutgoingRpcHandlers;

    // - Incoming RPC (server -> client) -

    public RpcSubscription Subscribe(ERpcId rpcId, Action<IncomingRpcArgs> handler)
    {
        return Handlers.Subscribe(rpcId, handler);
    }

    public IDisposable Bind(IRpcHandler handler, CancellationToken token = default, bool attachNow = true)
    {
        return Handlers.Bind(handler, token, attachNow);
    }

    public IDisposable Bind<TPayload>(ERpcId rpcId, Func<IncomingRpcArgs, TPayload> parser, Action<TPayload, IncomingRpcArgs> handler, CancellationToken token = default, string? name = null)
    {
        return Handlers.Bind(rpcId, parser, handler, token, name);
    }

    public async IAsyncEnumerable<IncomingRpcPayload> Stream(ERpcId rpcId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<IncomingRpcPayload> queue = new();
        using RpcSubscription subscription = Subscribe(rpcId, args => queue.Enqueue(IncomingRpcPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out IncomingRpcPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<TPayload> Stream<TPayload>(ERpcId rpcId, Func<IncomingRpcArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingRpcPayload payload in Stream(rpcId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    // - Outgoing RPC (client -> server) -

    public RpcSubscription SubscribeOutgoing(ERpcId rpcId, Action<OutgoingRpcArgs> handler)
    {
        return OutgoingHandlers.Subscribe(rpcId, handler);
    }

    public async IAsyncEnumerable<OutgoingRpcPayload> StreamOutgoing(ERpcId rpcId, [EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<OutgoingRpcPayload> queue = new();
        using RpcSubscription subscription = SubscribeOutgoing(rpcId, args => queue.Enqueue(OutgoingRpcPayload.From(args)));

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out OutgoingRpcPayload payload))
            {
                yield return payload;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<TPayload> StreamOutgoing<TPayload>(ERpcId rpcId, Func<OutgoingRpcArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingRpcPayload payload in StreamOutgoing(rpcId, token))
        {
            yield return payload.Parse(parser);
        }
    }
}
