using SFSharp.Interop.RakNet.Packets.Enum;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SFSharp;

public sealed class SFRpcParsers
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
        where TRpc : class, IParsedIncomingRpc
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
                if (parser.TryParse(args, out RpcParseResult result) && result.Rpc is TRpc rpc)
                {
                    handler(rpc);
                }
            }));
        }

        group.Link(token);
        return group;
    }

    public IDisposable BindOutgoing<TRpc>(Action<TRpc> handler, CancellationToken token = default)
        where TRpc : class, IParsedOutgoingRpc
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
                if (parser.TryParse(args, out RpcParseResult result) && result.Rpc is TRpc rpc)
                {
                    handler(rpc);
                }
            }));
        }

        group.Link(token);
        return group;
    }

    public async IAsyncEnumerable<TRpc> StreamIncoming<TRpc>([EnumeratorCancellation] CancellationToken token = default)
        where TRpc : class, IParsedIncomingRpc
    {
        ConcurrentQueue<TRpc> queue = new();
        using IDisposable binding = BindIncoming<TRpc>(rpc => queue.Enqueue(rpc), token);

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out TRpc? rpc))
            {
                yield return rpc;
            }

            await Task.Yield();
        }
    }

    public async IAsyncEnumerable<TRpc> StreamOutgoing<TRpc>([EnumeratorCancellation] CancellationToken token = default)
        where TRpc : class, IParsedOutgoingRpc
    {
        ConcurrentQueue<TRpc> queue = new();
        using IDisposable binding = BindOutgoing<TRpc>(rpc => queue.Enqueue(rpc), token);

        while (!token.IsCancellationRequested)
        {
            while (queue.TryDequeue(out TRpc? rpc))
            {
                yield return rpc;
            }

            await Task.Yield();
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
}
