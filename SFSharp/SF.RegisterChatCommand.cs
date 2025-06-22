using System.Runtime.InteropServices;

namespace SFSharp;

public static partial class SF
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate void CmdProc(byte* text);
    private unsafe static CmdProc CreateCmdProc(Action<string?> action)
    {
        return text =>
        {
            try
            {
                string? commandText = Marshal.PtrToStringAnsi((IntPtr)text);
                action(commandText);
            }
            catch (Exception ex)
            {
                SFCore.LogException(ex);
            }
        };
    }
    private static Dictionary<string, CmdProc> _commandProcedures = new Dictionary<string, CmdProc>();

    public unsafe static void RegisterChatCommand(string command, Action<string?> commandProcedure)
    {
        var proc = CreateCmdProc(commandProcedure);
        _commandProcedures[command] = proc;
        var pointer = (delegate* unmanaged[Cdecl]<byte*, void>)Marshal.GetFunctionPointerForDelegate(proc);
        SFCore.RegisterChatCommand(command, pointer);
    }

    public static unsafe void UnregisterChatCommand(string command)
    {
        _commandProcedures.Remove(command);
        SFCore.UnregisterChatCommand(command);
    }
}