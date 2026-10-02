using System.Runtime.CompilerServices;

namespace SFSharp.Runtime.Networking;

public sealed unsafe class SFRpc : ISFRpc
{
    public IDisposable RegisterIncomingFilter(int rpcId, SFRpcFilterCallback filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SFBootstrap.IncomingRpcFilters.Add(rpcId, (dataPtr, bitLength) =>
        {
            var span = new ReadOnlySpan<byte>((void*)dataPtr, (bitLength + 7) / 8);
            return filter(rpcId, span, bitLength);
        });
    }

    public IDisposable RegisterIncomingFilter(SFRpcFilterCallback filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SFBootstrap.IncomingRpcFilters.Add((id, dataPtr, bitLength) =>
        {
            var span = new ReadOnlySpan<byte>((void*)dataPtr, (bitLength + 7) / 8);
            return filter(id, span, bitLength);
        });
    }

    public IDisposable RegisterOutgoingFilter(int rpcId, SFRpcFilterCallback filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SFBootstrap.OutgoingRpcFilters.Add(rpcId, (dataPtr, bitLength) =>
        {
            var span = new ReadOnlySpan<byte>((void*)dataPtr, (bitLength + 7) / 8);
            return filter(rpcId, span, bitLength);
        });
    }

    public IDisposable RegisterOutgoingFilter(SFRpcFilterCallback filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        return SFBootstrap.OutgoingRpcFilters.Add((id, dataPtr, bitLength) =>
        {
            var span = new ReadOnlySpan<byte>((void*)dataPtr, (bitLength + 7) / 8);
            return filter(id, span, bitLength);
        });
    }

    public RpcHandlerManager Handlers => SFBootstrap.RpcHandlers;
    public OutgoingRpcManager OutgoingHandlers => SFBootstrap.OutgoingRpcHandlers;

    // - Incoming RPC (server -> client) -

    public RpcSubscription Subscribe(SampRpcId rpcId, Action<IncomingRpcArgs> handler)
    {
        return Handlers.Subscribe(rpcId, handler);
    }

    public IDisposable SubscribeIncoming(int rpcId, Action<IncomingRpcFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return Subscribe((SampRpcId)rpcId, args => handler(new IncomingRpcFrame(args.SampRpcId, IncomingRpcPayload.From(args).Data, args.DataBitOffset, args.DataBitLength)));
    }

    public IDisposable Bind(IRpcHandler handler, CancellationToken token = default, bool attachNow = true)
    {
        return Handlers.Bind(handler, token, attachNow);
    }

    public IDisposable Bind<TPayload>(SampRpcId rpcId, Func<IncomingRpcArgs, TPayload> parser, Action<TPayload, IncomingRpcArgs> handler, CancellationToken token = default, string? name = null)
    {
        return Handlers.Bind(rpcId, parser, handler, token, name);
    }

    public async IAsyncEnumerable<IncomingRpcPayload> Stream(SampRpcId rpcId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<IncomingRpcPayload>();
        using RpcSubscription subscription = Subscribe(rpcId, args => channel.Writer.TryWrite(IncomingRpcPayload.From(args)));

        try
        {
            await foreach (IncomingRpcPayload payload in channel.Reader.ReadAllAsync(token))
            {
                yield return payload;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }

    public async IAsyncEnumerable<TPayload> Stream<TPayload>(SampRpcId rpcId, Func<IncomingRpcArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingRpcPayload payload in Stream(rpcId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    // - Outgoing RPC (client -> server) -

    public RpcSubscription SubscribeOutgoing(SampRpcId rpcId, Action<OutgoingRpcArgs> handler)
    {
        return OutgoingHandlers.Subscribe(rpcId, handler);
    }

    public IDisposable SubscribeOutgoing(int rpcId, Action<OutgoingRpcFrame> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return SubscribeOutgoing((SampRpcId)rpcId, args => handler(new OutgoingRpcFrame(args.SampRpcId, OutgoingRpcPayload.From(args).Data, args.DataBitLength)));
    }

    public async IAsyncEnumerable<OutgoingRpcPayload> StreamOutgoing(SampRpcId rpcId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<OutgoingRpcPayload>();
        using RpcSubscription subscription = SubscribeOutgoing(rpcId, args => channel.Writer.TryWrite(OutgoingRpcPayload.From(args)));

        try
        {
            await foreach (OutgoingRpcPayload payload in channel.Reader.ReadAllAsync(token))
            {
                yield return payload;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }


    public async IAsyncEnumerable<TPayload> StreamOutgoing<TPayload>(SampRpcId rpcId, Func<OutgoingRpcArgs, TPayload> parser, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingRpcPayload payload in StreamOutgoing(rpcId, token))
        {
            yield return payload.Parse(parser);
        }
    }

    public async IAsyncEnumerable<IncomingRpcFrame> StreamIncoming(int rpcId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (IncomingRpcPayload payload in Stream((SampRpcId)rpcId, token))
        {
            yield return new IncomingRpcFrame((int)payload.SampRpcId, payload.Data, payload.DataBitOffset, payload.DataBitLength);
        }
    }

    public async IAsyncEnumerable<OutgoingRpcFrame> StreamOutgoing(int rpcId, [EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (OutgoingRpcPayload payload in StreamOutgoing((SampRpcId)rpcId, token))
        {
            yield return new OutgoingRpcFrame((int)payload.SampRpcId, payload.Data, payload.DataBitLength);
        }
    }

    // - RPC filters (synchronous, run on hook thread) -

    public IDisposable RegisterOutgoingFilter(SampRpcId rpcId, Func<nint, int, bool> filter)
    {
        return SFBootstrap.OutgoingRpcFilters.Add((int)rpcId, filter);
    }

    public IDisposable RegisterOutgoingFilter(Func<int, nint, int, bool> filter)
    {
        return SFBootstrap.OutgoingRpcFilters.Add(filter);
    }

    public IDisposable RegisterIncomingFilter(SampRpcId rpcId, Func<nint, int, bool> filter)
    {
        return SFBootstrap.IncomingRpcFilters.Add((int)rpcId, filter);
    }

    public IDisposable RegisterIncomingFilter(Func<int, nint, int, bool> filter)
    {
        return SFBootstrap.IncomingRpcFilters.Add(filter);
    }
}
