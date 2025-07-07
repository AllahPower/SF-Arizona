using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;

namespace SFSharp;

public record SFChatEntry(EntryType Type, string? Text, string? Prefix, uint TextColor, uint PrefixColor);

public class SFChat : ISFComponent
{
    void ISFComponent.Initialize()
    {
        var subHook = new SubHook();
        HookManager.CChatAddEntry.AddSubHook(subHook);
        HookManager.CInputGetCommandHandler.AddSubHook(subHook);
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

    private class CommandTaskSource : ICommandTaskSource
    {
        private readonly string _commandName;
        private readonly Channel<string?> _channel;

        private ChannelReader<string?> Reader => field ??= _channel.Reader;
        private ChannelWriter<string?> Writer => field ??= _channel.Writer;

        public CommandTaskSource(string commandName)
        {
            _commandName = commandName;
            _channel = Channel.CreateBounded<string?>(new BoundedChannelOptions(1)
            {
                AllowSynchronousContinuations = false,
                FullMode = BoundedChannelFullMode.DropWrite,
                SingleReader = true,
                SingleWriter = true
            });
        }

        public void OnCommand(string? args)
        {
            Writer.TryWrite(args);
        }

        public ValueTask<string?> WaitForCommandAsync(CancellationToken token = default)
        {
            return Reader.ReadAsync(token);
        }

        public IAsyncEnumerable<string?> StreamCommandsAsync(CancellationToken token = default)
        {
            return Reader.ReadAllAsync(token);
        }

        public void Dispose()
        {
            Writer.Complete();
            _taskSourcesByCommand.Remove(_commandName);
        }

        
    }
    private static Dictionary<string, CommandTaskSource> _taskSourcesByCommand = new();
    private static string? _lastCommand = null;
    public ICommandTaskSource RegisterChatCommand(string command)
    {
        var taskSource = new CommandTaskSource(command);
        _taskSourcesByCommand.Add(command, taskSource);
        return taskSource;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe void OnCommand(byte* text)
    {
        if (_lastCommand is null) throw new UnreachableException();
        var args = AnsiString.Decode(text);

        _taskSourcesByCommand[_lastCommand].OnCommand(args);
        _lastCommand = null;
    }

    private unsafe class SubHook :
        ISubHook<CChatAddEntryArgs, NoRetValue>,
        ISubHook<CInputGetCommandHandlerArgs, CInputGetCommandHandlerRetValue>
    {
        NoRetValue ISubHook<CChatAddEntryArgs, NoRetValue>.Process(CChatAddEntryArgs args, Func<CChatAddEntryArgs, NoRetValue> next)
        {
            next(args);
            var entry = new SFChatEntry((EntryType)args.Type, args.Text, args.Prefix, args.TextColor, args.PrefixColor);
            foreach (var queue in _consumerQueues)
            {
                queue.Enqueue(entry);
            }
            return default;
        }

        CInputGetCommandHandlerRetValue ISubHook<CInputGetCommandHandlerArgs, CInputGetCommandHandlerRetValue>.Process(CInputGetCommandHandlerArgs args, Func<CInputGetCommandHandlerArgs, CInputGetCommandHandlerRetValue> next)
        {
            if (_taskSourcesByCommand.ContainsKey(args.CommandName))
            {
                _lastCommand = args.CommandName;
                return (delegate* unmanaged[Cdecl]<byte*, void>)&OnCommand;
            }
            return next(args);
        }
    }
}

public interface ICommandTaskSource : IDisposable
{
    ValueTask<string?> WaitForCommandAsync(CancellationToken token = default);
    IAsyncEnumerable<string?> StreamCommandsAsync(CancellationToken token = default);
}