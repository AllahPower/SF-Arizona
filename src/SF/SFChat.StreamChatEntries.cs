using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SFSharp;

public record ChatEntry(EntryType Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);

public enum ServerChatKind
{
    Chat,
    ClientMessage
}

public record ServerChatEntry(ServerChatKind Kind, RpcId RpcId, ChatEntry Entry)
{
    public EntryType Type => Entry.Type;
    public string? Text => Entry.Text;
    public string? Prefix => Entry.Prefix;
    public uint TextColor => Entry.TextColor;
    public uint PrefixColor => Entry.PrefixColor;
}

public unsafe partial class SFChat : ISubHook<CChatAddEntryArgs, NoRetValue>
{
    private static readonly List<ConcurrentQueue<ServerChatEntry>> _serverConsumerQueues = new();
    private static readonly List<ConcurrentQueue<ChatEntry>> _localConsumerQueues = new();
    private static int _suppressOwnLocalAddEntry;
    private bool _rpcBindingsRegistered;

    public void RegisterRpcBindings(RpcHandlerManager manager)
    {
        if (_rpcBindingsRegistered)
        {
            return;
        }

        manager.Bind(RpcId.Chat, SampRpc.ParseChatMessage, static (payload, args) =>
        {
            // Chat prefixColor is 0xRRGGBBAA (PAWN), convert to 0xAARRGGBB (internal)
            uint prefixColor = (payload.PrefixColor >> 8) | ((payload.PrefixColor & 0xFF) << 24);
            ChatEntry entry = new(EntryType.Chat, payload.Text, payload.Prefix, 0xFFFFFFFF, prefixColor);
            SF.Chat.PublishServerChatEntry(new ServerChatEntry(ServerChatKind.Chat, RpcId.Chat, entry));
        }, name: "IncomingChatMessageRpc");

        manager.Bind(RpcId.ClientMessage, SampRpc.ParseClientMessage, static (payload, args) =>
        {
            // SendClientMessage color is 0xRRGGBBAA (PAWN), convert to 0xAARRGGBB (internal)
            uint color = (payload.Color >> 8) | ((payload.Color & 0xFF) << 24);
            ChatEntry entry = new(EntryType.Info, payload.Text, null, color, 0);
            SF.Chat.PublishServerChatEntry(new ServerChatEntry(ServerChatKind.ClientMessage, RpcId.ClientMessage, entry));
        }, name: "IncomingClientMessageRpc");

        _rpcBindingsRegistered = true;
    }

    public async IAsyncEnumerable<ServerChatEntry> StreamServerChatEntries([EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<ServerChatEntry> queue = new();
        _serverConsumerQueues.Add(queue);
        try
        {
            while (!token.IsCancellationRequested)
            {
                while (queue.TryDequeue(out ServerChatEntry? entry))
                {
                    yield return entry;
                }

                await Task.Yield();
            }
        }
        finally
        {
            _serverConsumerQueues.Remove(queue);
        }
    }

    public async IAsyncEnumerable<ChatEntry> StreamChatEntries([EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (ServerChatEntry entry in StreamServerChatEntries(token))
        {
            yield return entry.Entry;
        }
    }

    public async IAsyncEnumerable<ChatEntry> StreamLocalChatEntries([EnumeratorCancellation] CancellationToken token = default)
    {
        ConcurrentQueue<ChatEntry> queue = new();
        _localConsumerQueues.Add(queue);
        try
        {
            while (!token.IsCancellationRequested)
            {
                while (queue.TryDequeue(out ChatEntry? entry))
                {
                    yield return entry;
                }

                await Task.Yield();
            }
        }
        finally
        {
            _localConsumerQueues.Remove(queue);
        }
    }

    internal static IDisposable SuppressOwnLocalAddEntry()
    {
        Interlocked.Increment(ref _suppressOwnLocalAddEntry);
        return new ActionOnDispose(() => Interlocked.Decrement(ref _suppressOwnLocalAddEntry));
    }

    internal void PublishServerChatEntry(ServerChatEntry entry)
    {
        //SFLog.Info($"Server chat entry kind={entry.Kind} rpcId={(int)entry.RpcId} type={entry.Type} textColor=0x{entry.TextColor:X8} prefixColor=0x{entry.PrefixColor:X8} prefix={entry.Prefix ?? "<null>"} text={entry.Text ?? "<null>"}");
        foreach (ConcurrentQueue<ServerChatEntry> queue in _serverConsumerQueues)
        {
            queue.Enqueue(entry);
        }
    }

    internal void PublishLocalChatEntry(ChatEntry entry)
    {
        foreach (ConcurrentQueue<ChatEntry> queue in _localConsumerQueues)
        {
            queue.Enqueue(entry);
        }
    }

    NoRetValue ISubHook<CChatAddEntryArgs, NoRetValue>.Process(CChatAddEntryArgs args, Func<CChatAddEntryArgs, NoRetValue> next)
    {
        next(args);
        if (Volatile.Read(ref _suppressOwnLocalAddEntry) > 0)
        {
            return default;
        }

        ChatEntry entry = new((EntryType)args.Type, args.Text, args.Prefix, args.TextColor, args.PrefixColor);
        PublishLocalChatEntry(entry);
        return default;
    }

    private sealed class ActionOnDispose : IDisposable
    {
        private readonly Action _action;
        private int _disposed;

        public ActionOnDispose(Action action)
        {
            _action = action;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _action();
        }
    }
}


