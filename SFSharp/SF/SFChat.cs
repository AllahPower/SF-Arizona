using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public record SFChatEntry(EntryType Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);

public class SFChat : ISFComponent
{
    void ISFComponent.Initialize()
    {
        HookManager.CChatAddEntry.AddSubHook(new SubHook());
    }

    public void Send(string message)
    {
        if (message.StartsWith('/'))
        {
            CInput.Instance.Send(message);
        }
        else
        {
            CLocalPlayer.Instance.Chat(message);
        }
    }

    public void Add(string text, uint textColor = 0xFFAAAAAA, string? prefix = null, uint prefixColor = 0xFFAAAAAA)
    {
        CChat.Instance.AddEntry(EntryType.Debug, text, prefix, textColor, prefixColor);
    }

    private static readonly List<ConcurrentQueue<SFChatEntry>> _consumerQueues = new();
    public async IAsyncEnumerable<SFChatEntry> StreamChatEntries([EnumeratorCancellation] CancellationToken token = default)
    {
        var queue = new ConcurrentQueue<SFChatEntry>();
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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate void CmdProc(byte* text);
    private static Dictionary<string, CmdProc> _commandProcedures = new Dictionary<string, CmdProc>();
    public unsafe void RegisterChatCommand(string command, Action<string?> commandProcedure)
    {
        var proc = new CmdProc(text =>
        {
            try
            {
                string? commandText = Marshal.PtrToStringAnsi((IntPtr)text);
                commandProcedure(commandText);
            }
            catch (Exception ex)
            {
                SFBootstrap.ProcessException(ex);
            }
        });
        _commandProcedures[command] = proc;
        var pointer = (delegate* unmanaged[Cdecl]<byte*, void>)Marshal.GetFunctionPointerForDelegate(proc);
        CInput.Instance.AddCommand(command, pointer);
    }

    private class SubHook : ISubHook<CChatAddEntryArgs, NoRetValue>
    {
        public NoRetValue Process(CChatAddEntryArgs args, Func<CChatAddEntryArgs, NoRetValue> next)
        {
            next(args);
            var entry = new SFChatEntry((EntryType)args.Type, args.Text, args.Prefix, args.TextColor, args.PrefixColor);
            foreach (var queue in _consumerQueues)
            {
                queue.Enqueue(entry);
            }
            return default;
        }
    }
}
