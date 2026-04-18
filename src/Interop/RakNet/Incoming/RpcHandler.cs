using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Network.RakNet.Incoming;

public interface IRpcHandler : IDisposable
{
    ERpcId ERpcId { get; }
    string Name { get; }
    bool IsAttached { get; }

    void Attach(RpcHandlerManager manager);
    void Detach();
}

public abstract class RpcHandlerBase : IRpcHandler
{
    private RpcSubscription? _subscription;

    protected RpcHandlerManager? Manager { get; private set; }

    public abstract ERpcId ERpcId { get; }
    public virtual string Name => GetType().Name;
    public bool IsAttached => _subscription is not null;

    public void Attach(RpcHandlerManager manager)
    {
        if (IsAttached)
        {
            throw new InvalidOperationException($"RPC handler {Name} is already attached.");
        }

        Manager = manager;
        _subscription = manager.Subscribe(ERpcId, DispatchIncoming);
        OnAttached();
        SFLog.Debug($"RpcHandler attached name={Name} rpcId={(int)ERpcId}");
    }

    public void Detach()
    {
        if (_subscription is null)
        {
            return;
        }

        _subscription.Dispose();
        _subscription = null;
        OnDetached();
        Manager = null;
        SFLog.Debug($"RpcHandler detached name={Name} rpcId={(int)ERpcId}");
    }

    public void Dispose()
    {
        Detach();
    }

    protected virtual void OnAttached()
    {
    }

    protected virtual void OnDetached()
    {
    }

    protected abstract void OnIncoming(IncomingRpcArgs args);

    private void DispatchIncoming(IncomingRpcArgs args)
    {
        try
        {
            OnIncoming(args);
        }
        catch (Exception ex)
        {
            SFLog.Error(ex, $"RpcHandler dispatch failed name={Name} rpcId={args.ERpcId}");
        }
    }
}

public abstract class RpcHandler<TPayload> : RpcHandlerBase
{
    protected sealed override void OnIncoming(IncomingRpcArgs args)
    {
        TPayload payload = Parse(args);
        Handle(payload, args);
    }

    protected abstract TPayload Parse(IncomingRpcArgs args);
    protected abstract void Handle(TPayload payload, IncomingRpcArgs args);
}

public sealed class DelegateRpcHandler<TPayload> : RpcHandler<TPayload>
{
    private readonly ERpcId _rpcId;
    private readonly string _name;
    private readonly Func<IncomingRpcArgs, TPayload> _parser;
    private readonly Action<TPayload, IncomingRpcArgs> _handler;

    public DelegateRpcHandler(ERpcId rpcId, Func<IncomingRpcArgs, TPayload> parser, Action<TPayload, IncomingRpcArgs> handler, string? name = null)
    {
        _rpcId = rpcId;
        _parser = parser;
        _handler = handler;
        _name = string.IsNullOrWhiteSpace(name) ? $"DelegateRpcHandler<{typeof(TPayload).Name}>" : name;
    }

    public override ERpcId ERpcId => _rpcId;
    public override string Name => _name;

    protected override TPayload Parse(IncomingRpcArgs args)
    {
        return _parser(args);
    }

    protected override void Handle(TPayload payload, IncomingRpcArgs args)
    {
        _handler(payload, args);
    }
}
