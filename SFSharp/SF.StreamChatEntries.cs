using System.Runtime.CompilerServices;

namespace SFSharp;

public record SFChatEntry(string? Text, uint TextColor);

public static partial class SF
{
    private static readonly List<Queue<SFChatEntry>> _consumerQueues = new();

    public class SFChatSubHook : ISubHook<CChatAddEntryArgs>
    {
        public void Process(CChatAddEntryArgs args, Action<CChatAddEntryArgs> next)
        {
            next(args);
            var entry = new SFChatEntry(args.Text, args.TextColor);
            BeginInvoke(_ =>
            {
                foreach(var queue in _consumerQueues)
                {
                    queue.Enqueue(entry);
                }
                SFDebug.Log(entry.Text);
            });
        }
    }

    public static void InstallChatHook()
    {
        HookManager.CChatAddEntry.AddSubHook(new SFChatSubHook());
    }

    public static async IAsyncEnumerable<SFChatEntry> StreamChatEntries([EnumeratorCancellation] CancellationToken token = default)
    {
        var queue = new Queue<SFChatEntry>();
        _consumerQueues.Add(queue);
        try
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
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
}
