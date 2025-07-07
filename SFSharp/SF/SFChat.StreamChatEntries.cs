using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SFSharp;

public record ChatEntry(EntryType Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);

public unsafe partial class SFChat
{
    private static readonly List<ConcurrentQueue<ChatEntry>> _consumerQueues = new();
    public async IAsyncEnumerable<ChatEntry> StreamChatEntries([EnumeratorCancellation] CancellationToken token = default)
    {
        var queue = new ConcurrentQueue<ChatEntry>();
        _consumerQueues.Add(queue);
        try
        {
            while (!token.IsCancellationRequested)
            {
                while (queue.TryDequeue(out var entry))
                {
                    yield return entry;
                }
                await Task.Yield();
            }
        }
        finally
        {
            _consumerQueues.Remove(queue);
        }
    }

    NoRetValue ISubHook<CChatAddEntryArgs, NoRetValue>.Process(CChatAddEntryArgs args, Func<CChatAddEntryArgs, NoRetValue> next)
    {
        next(args);
        var entry = new ChatEntry((EntryType)args.Type, args.Text, args.Prefix, args.TextColor, args.PrefixColor);
        foreach (var queue in _consumerQueues)
        {
            queue.Enqueue(entry);
        }
        return default;
    }
}