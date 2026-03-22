using SFSharp.Interop.RakNet.Packets.Enum;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace SFSharp;

public sealed class RpcHandlerManager : IDisposable
{
    private readonly Lock _sync = new();
    private readonly Dictionary<int, List<Action<IncomingRpcArgs>>> _listeners = new();
    private readonly List<IRpcHandler> _handlers = new();
    private bool _started;

    public IReadOnlyList<IRpcHandler> Handlers
    {
        get
        {
            lock (_sync)
            {
                return _handlers.ToArray();
            }
        }
    }

    public bool HasSubscribers(ERpcId rpcId)
    {
        return HasSubscribers((int)rpcId);
    }

    public bool HasSubscribers(int rpcId)
    {
        lock (_sync)
        {
            return _listeners.TryGetValue(rpcId, out List<Action<IncomingRpcArgs>>? list) && list.Count > 0;
        }
    }

    public RpcSubscription Subscribe(ERpcId rpcId, Action<IncomingRpcArgs> handler)
    {
        return Subscribe((int)rpcId, handler);
    }

    public RpcSubscription Subscribe(int rpcId, Action<IncomingRpcArgs> handler)
    {
        lock (_sync)
        {
            if (!_listeners.TryGetValue(rpcId, out List<Action<IncomingRpcArgs>>? list))
            {
                list = new List<Action<IncomingRpcArgs>>();
                _listeners[rpcId] = list;
            }

            list.Add(handler);
        }

        return new RpcSubscription(() =>
        {
            lock (_sync)
            {
                if (!_listeners.TryGetValue(rpcId, out List<Action<IncomingRpcArgs>>? list))
                {
                    return;
                }

                list.Remove(handler);
                if (list.Count == 0)
                {
                    _listeners.Remove(rpcId);
                }
            }
        });
    }

    public T Register<T>(bool attachNow = true) where T : IRpcHandler, new()
    {
        T handler = new();
        Register(handler, attachNow);
        return handler;
    }

    public void Register(IRpcHandler handler, bool attachNow = true)
    {
        lock (_sync)
        {
            if (_handlers.Contains(handler))
            {
                throw new InvalidOperationException($"RPC handler {handler.Name} is already registered.");
            }

            _handlers.Add(handler);
        }

        SFLog.Info($"RpcHandlerManager registered name={handler.Name} rpcId={(int)handler.ERpcId}");

        if (_started && attachNow)
        {
            handler.Attach(this);
        }
    }

    public IDisposable Bind(IRpcHandler handler, CancellationToken token = default, bool attachNow = true)
    {
        Register(handler, attachNow);
        RpcBinding binding = new(this, handler);
        if (token.CanBeCanceled)
        {
            binding.Link(token);
        }

        return binding;
    }

    public IDisposable Bind<TPayload>(ERpcId rpcId, Func<IncomingRpcArgs, TPayload> parser, Action<TPayload, IncomingRpcArgs> handler, CancellationToken token = default, string? name = null)
    {
        DelegateRpcHandler<TPayload> rpcHandler = new(rpcId, parser, handler, name);
        return Bind(rpcHandler, token);
    }

    public bool Unregister(IRpcHandler handler)
    {
        lock (_sync)
        {
            if (!_handlers.Remove(handler))
            {
                return false;
            }
        }

        handler.Detach();
        SFLog.Info($"RpcHandlerManager unregistered name={handler.Name} rpcId={(int)handler.ERpcId}");
        return true;
    }

    public void StartAll()
    {
        IRpcHandler[] snapshot;
        lock (_sync)
        {
            snapshot = _handlers.ToArray();
            _started = true;
        }

        foreach (IRpcHandler handler in snapshot)
        {
            if (!handler.IsAttached)
            {
                handler.Attach(this);
            }
        }

        SFLog.Info($"RpcHandlerManager started count={snapshot.Length}");
    }

    public void StopAll()
    {
        IRpcHandler[] snapshot;
        lock (_sync)
        {
            snapshot = _handlers.ToArray();
            _started = false;
        }

        foreach (IRpcHandler handler in snapshot)
        {
            handler.Detach();
        }

        SFLog.Info($"RpcHandlerManager stopped count={snapshot.Length}");
    }

    internal void DispatchIncoming(int rpcId, byte[] packet, int payloadBitOffset, int payloadBitLength)
    {
        Action<IncomingRpcArgs>[] snapshot;
        lock (_sync)
        {
            if (!_listeners.TryGetValue(rpcId, out List<Action<IncomingRpcArgs>>? list) || list.Count == 0)
            {
                return;
            }

            snapshot = list.ToArray();
        }

        unsafe
        {
            fixed (byte* packetPtr = packet)
            {
                IncomingRpcArgs args = new(rpcId, (nint)packetPtr, payloadBitOffset, payloadBitLength);
                foreach (Action<IncomingRpcArgs> listener in snapshot)
                {
                    listener(args);
                }
            }
        }
    }

    public void Dispose()
    {
        StopAll();
    }

    private sealed class RpcBinding : IDisposable
    {
        private readonly RpcHandlerManager _owner;
        private readonly IRpcHandler _handler;
        private CancellationTokenRegistration _tokenRegistration;
        private bool _disposed;

        public RpcBinding(RpcHandlerManager owner, IRpcHandler handler)
        {
            _owner = owner;
            _handler = handler;
        }

        public void Link(CancellationToken token)
        {
            _tokenRegistration = token.Register(Dispose);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _tokenRegistration.Dispose();
            _owner.Unregister(_handler);
        }
    }
}
