using SFSharp.Interop.RakNet.Packets.Enum;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SFSharp;

public unsafe partial class SFChat : ISubHook<CChatAddEntryArgs, NoRetValue>
{
    private static readonly List<ChannelWriter<ServerChatEntry>> _serverConsumerWriters = new();
    private static readonly List<ChannelWriter<ChatEntry>> _localConsumerWriters = new();
    private static int _suppressOwnLocalAddEntry;
    private bool _rpcBindingsRegistered;

    public void RegisterRpcBindings(RpcHandlerManager manager)
    {
        if (_rpcBindingsRegistered)
        {
            return;
        }

        manager.Bind(ERpcId.Chat, SampRpc.ParseChatMessage, static (payload, args) =>
        {
            // Chat prefixColor is 0xRRGGBBAA (PAWN), convert to 0xAARRGGBB (internal)
            uint prefixColor = (payload.PrefixColor >> 8) | ((payload.PrefixColor & 0xFF) << 24);
            ChatEntry entry = new(EntryType.Chat, payload.Text, payload.Prefix, 0xFFFFFFFF, prefixColor);
            SF.Chat.PublishServerChatEntry(new ServerChatEntry(ServerChatKind.Chat, ERpcId.Chat, entry));
        }, name: "IncomingChatMessageRpc");

        manager.Bind(ERpcId.ClientMessage, SampRpc.ParseClientMessage, static (payload, args) =>
        {
            // SendClientMessage color is 0xRRGGBBAA (PAWN), convert to 0xAARRGGBB (internal)
            uint color = (payload.Color >> 8) | ((payload.Color & 0xFF) << 24);
            ChatEntry entry = new(EntryType.Info, payload.Text, null, color, 0);
            SF.Chat.PublishServerChatEntry(new ServerChatEntry(ServerChatKind.ClientMessage, ERpcId.ClientMessage, entry));
        }, name: "IncomingClientMessageRpc");

        _rpcBindingsRegistered = true;
    }

    public async IAsyncEnumerable<ServerChatEntry> StreamServerChatEntries([EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<ServerChatEntry>();

        _serverConsumerWriters.Add(channel.Writer);
        try
        {
            await foreach (ServerChatEntry entry in channel.Reader.ReadAllAsync(token))
            {
                yield return entry;
            }
        }
        finally
        {
            _serverConsumerWriters.Remove(channel.Writer);
            channel.Writer.TryComplete();
        }
    }

    public async IAsyncEnumerable<ChatEntry> StreamChatEntries([EnumeratorCancellation] CancellationToken token = default)
    {
        await foreach (ServerChatEntry entry in StreamServerChatEntries(token))
        {
            yield return entry.Entry;
        }
    }

    public IAsyncEnumerable<ChatEntry> StreamEntries(CancellationToken token = default) => StreamLocalChatEntries(token);

    public IAsyncEnumerable<ServerChatEntry> StreamServerEntries(CancellationToken token = default) => StreamServerChatEntries(token);

    public async IAsyncEnumerable<ChatEntry> StreamLocalChatEntries([EnumeratorCancellation] CancellationToken token = default)
    {
        var channel = SFChannel.CreateUnbounded<ChatEntry>();

        _localConsumerWriters.Add(channel.Writer);
        try
        {
            await foreach (ChatEntry entry in channel.Reader.ReadAllAsync(token))
            {
                yield return entry;
            }
        }
        finally
        {
            _localConsumerWriters.Remove(channel.Writer);
            channel.Writer.TryComplete();
        }
    }

    internal static IDisposable SuppressOwnLocalAddEntry()
    {
        Interlocked.Increment(ref _suppressOwnLocalAddEntry);
        return new ActionOnDispose(() => Interlocked.Decrement(ref _suppressOwnLocalAddEntry));
    }

    internal void PublishServerChatEntry(ServerChatEntry entry)
    {
        foreach (ChannelWriter<ServerChatEntry> writer in _serverConsumerWriters)
        {
            writer.TryWrite(entry);
        }
    }

    internal void PublishLocalChatEntry(ChatEntry entry)
    {
        foreach (ChannelWriter<ChatEntry> writer in _localConsumerWriters)
        {
            writer.TryWrite(entry);
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


