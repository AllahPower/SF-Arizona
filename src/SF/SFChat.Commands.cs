using System.Diagnostics;

namespace SFSharp;

public partial class SFChat : ISubHook<CInputCommandSendArgs, bool>
{
    private record CommandRegistration(string Name, Action<string?> Callback) : IDisposable
    {
        public void OnCommand(string? args) => Callback(args);

        public void Dispose()
        {
            SFLog.Info($"UnregisterChatCommand name={Name}");
            _taskSourcesByCommand.Remove(Name);
        }
    }

    private static Dictionary<string, CommandRegistration> _taskSourcesByCommand = new();
    private static string? _lastCommand = null;

    public IDisposable RegisterChatCommand(string command, Action<string?> commandCallback)
    {
        SFLog.Info($"RegisterChatCommand name={command}");
        var registration = new CommandRegistration(command, commandCallback);
        _taskSourcesByCommand.Add(command, registration);
        return registration;
    }

    bool ISubHook<CInputCommandSendArgs, bool>.Process(CInputCommandSendArgs args, Func<CInputCommandSendArgs, bool> next)
    {
        if (!args.Text.StartsWith('/'))
        {
            return next(args);
        }

        string commandLine = args.Text[1..];
        int separatorIndex = commandLine.IndexOf(' ');
        string commandName = separatorIndex >= 0 ? commandLine[..separatorIndex] : commandLine;
        string? commandArgs = separatorIndex >= 0 ? commandLine[(separatorIndex + 1)..] : null;

        SFLog.Info($"Client command send command={commandName} args={commandArgs ?? "<null>"}");
        if (_taskSourcesByCommand.TryGetValue(commandName, out CommandRegistration? registration))
        {
            _lastCommand = commandName;
            SFLog.Info($"Client command intercepted command={commandName}");
            try
            {
                registration.OnCommand(commandArgs);
            }
            catch (Exception ex)
            {
                SFBootstrap.ProcessException(ex);
            }
            finally
            {
                _lastCommand = null;
            }

            return true;
        }

        SFLog.Info($"Client command pass-through command={commandName}");
        return next(args);
    }
}
