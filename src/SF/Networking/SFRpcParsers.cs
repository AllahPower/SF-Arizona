using System.Runtime.CompilerServices;

namespace SFSharp.Runtime.Network;

public sealed class SFRpcParsers : ISFRpcParsers
{
    private static readonly Lazy<RpcParserRegistry> _registry = new(RpcParserCatalog.CreateDefaultRegistry);

    public RpcParserRegistry Registry => _registry.Value;

    public bool TryParseIncoming(IncomingRpcArgs args, out RpcParseResult result)
    {
        return Registry.TryParseIncoming(args, out result);
    }

    public bool TryParseOutgoing(OutgoingRpcArgs args, out RpcParseResult result)
    {
        return Registry.TryParseOutgoing(args, out result);
    }

    public IDisposable BindIncoming<TRpc>(Action<TRpc> handler, CancellationToken token = default)
    {
        IReadOnlyList<RpcParserRegistry.IncomingRpcRoute> routes = Registry.GetIncomingRoutes<TRpc>();
        if (routes.Count == 0)
        {
            throw new InvalidOperationException($"Incoming RPC parser for {typeof(TRpc).FullName} is not registered.");
        }

        SubscriptionGroup group = new();
        foreach (RpcParserRegistry.IncomingRpcRoute route in routes)
        {
            IIncomingRpcParser parser = (IIncomingRpcParser)route.Parser;
            group.Add(SF.Rpc.Subscribe(route.ERpcId, args =>
            {
                if (parser.TryParse(args, out RpcParseResult result) && TryExtractIncoming(result, out TRpc rpc))
                {
                    handler(rpc);
                }
            }));
        }

        group.Link(token);
        return group;
    }

    public IDisposable BindOutgoing<TRpc>(Action<TRpc> handler, CancellationToken token = default)
    {
        IReadOnlyList<RpcParserRegistry.OutgoingRpcRoute> routes = Registry.GetOutgoingRoutes<TRpc>();
        if (routes.Count == 0)
        {
            throw new InvalidOperationException($"Outgoing RPC parser for {typeof(TRpc).FullName} is not registered.");
        }

        SubscriptionGroup group = new();
        foreach (RpcParserRegistry.OutgoingRpcRoute route in routes)
        {
            IOutgoingRpcParser parser = (IOutgoingRpcParser)route.Parser;
            group.Add(SF.Rpc.SubscribeOutgoing(route.ERpcId, args =>
            {
                if (parser.TryParse(args, out RpcParseResult result) && TryExtractOutgoing(result, out TRpc rpc))
                {
                    handler(rpc);
                }
            }));
        }

        group.Link(token);
        return group;
    }

    public async IAsyncEnumerable<TRpc> StreamIncoming<TRpc>([EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<TRpc>();
        using IDisposable binding = BindIncoming<TRpc>(rpc => channel.Writer.TryWrite(rpc), token);

        try
        {
            await foreach (TRpc rpc in channel.Reader.ReadAllAsync(token))
            {
                yield return rpc;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }

    public async IAsyncEnumerable<TRpc> StreamOutgoing<TRpc>([EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<TRpc>();
        using IDisposable binding = BindOutgoing<TRpc>(rpc => channel.Writer.TryWrite(rpc), token);

        try
        {
            await foreach (TRpc rpc in channel.Reader.ReadAllAsync(token))
            {
                yield return rpc;
            }
        }
        finally
        {
            channel.Writer.TryComplete();
        }
    }


    private sealed class SubscriptionGroup : IDisposable
    {
        private readonly List<IDisposable> _items = new();
        private CancellationTokenRegistration _tokenRegistration;
        private bool _disposed;

        public void Add(RpcSubscription subscription)
        {
            _items.Add(subscription);
        }

        public void Link(CancellationToken token)
        {
            if (token.CanBeCanceled)
            {
                _tokenRegistration = token.Register(Dispose);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _tokenRegistration.Dispose();
            foreach (IDisposable item in _items)
            {
                item.Dispose();
            }

            _items.Clear();
        }
    }

    private static bool TryExtractIncoming<TRpc>(RpcParseResult result, out TRpc rpc)
    {
        if (result.Rpc is TRpc direct)
        {
            rpc = direct;
            return true;
        }

        if (result.Rpc is IncomingRpc<TRpc> wrapped)
        {
            rpc = wrapped.Payload;
            return true;
        }

        rpc = default!;
        return false;
    }

    private static bool TryExtractOutgoing<TRpc>(RpcParseResult result, out TRpc rpc)
    {
        if (result.Rpc is TRpc direct)
        {
            rpc = direct;
            return true;
        }

        if (result.Rpc is OutgoingRpc<TRpc> wrapped)
        {
            rpc = wrapped.Payload;
            return true;
        }

        rpc = default!;
        return false;
    }
}
