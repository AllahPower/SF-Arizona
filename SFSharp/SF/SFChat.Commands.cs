using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using System.Threading.Tasks.Sources;

namespace SFSharp;

public partial class SFChat
{
	private record CommandRegistration(string Name, Action<string?> Callback) : IDisposable
	{
        public void OnCommand(string? args) => Callback(args);
        public void Dispose() => _taskSourcesByCommand.Remove(Name);
    }

	private static Dictionary<string, CommandRegistration> _taskSourcesByCommand = new();
	private static string? _lastCommand = null;
	public IDisposable RegisterChatCommand(string command, Action<string?> commandCallback)
	{
		var registration = new CommandRegistration(command, commandCallback);
		_taskSourcesByCommand.Add(command, registration);
		return registration;
	}


    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe void OnCommand(byte* text)
	{
		if (_lastCommand is null) throw new UnreachableException();
		var args = AnsiString.Decode(text);
        try
        {
            _taskSourcesByCommand[_lastCommand].OnCommand(args);
        }
        catch (Exception ex)
        {
            SFBootstrap.ProcessException(ex);
        }
		_lastCommand = null;
	}

    unsafe CInputGetCommandHandlerRetValue ISubHook<CInputGetCommandHandlerArgs, CInputGetCommandHandlerRetValue>.Process(CInputGetCommandHandlerArgs args, Func<CInputGetCommandHandlerArgs, CInputGetCommandHandlerRetValue> next)
    {
        if (_taskSourcesByCommand.ContainsKey(args.CommandName))
        {
            _lastCommand = args.CommandName;
            return (delegate* unmanaged[Cdecl]<byte*, void>)&OnCommand;
        }
        return next(args);
    }
}