using SFSharp;
using System.Runtime.InteropServices;

using unsafe SendDelegate = delegate* unmanaged[Thiscall]<CInput*, byte*, void>;
using unsafe AddCommandDelegate = delegate* unmanaged[Thiscall]<CInput*, byte*, delegate* unmanaged[Cdecl]<byte*, void>, void>;
using System.Runtime.CompilerServices;

[StructLayout(LayoutKind.Explicit, Size = 6908, Pack = 1)]
public unsafe struct CInput
{
    private static readonly CInput* _instance = *(CInput**)HookHelper.GetFunctionPtr("samp.dll", 0x26EB84);
    public static ref CInput Instance => ref *_instance;

    public CommandManager Commands => new(_instance);

    private static readonly SendDelegate _send = (SendDelegate)HookHelper.GetFunctionPtr("samp.dll", 0x69900);
    public void Send(string text)
    {
        using var textAnsi = AnsiString.Encode(text);
        _send(_instance, textAnsi);
    }

    private static readonly AddCommandDelegate _addCommand = (AddCommandDelegate)HookHelper.GetFunctionPtr("samp.dll", 0x69770);
    public void AddCommand(string command, delegate* unmanaged[Cdecl]<byte*, void> callback)
    {
        using var commandAnsi = AnsiString.Encode(command);
        _addCommand(_instance, commandAnsi, callback);
    }
}

public readonly unsafe struct CommandManager
{
    public const int MAX_CLIENT_CMDS = 144;
    private const int MAX_CMD_LENGTH = 32;

    private readonly CInput* _instance;

    public CommandManager(CInput* instance)
    {
        _instance = instance;
    }

    public int CommandCount => *(int*)((uint)_instance + 5340);

    private Span<uint> GetCommands()
    {
        var commandsArrayPtr = (uint*)((uint)_instance + 12);
        return new(commandsArrayPtr, MAX_CLIENT_CMDS);
    }

    private void GetCommandNames(Span<uint> destination)
    {
        var commandNamesArrayPtr = (uint)_instance + 588;
        for (int i = 0; i < MAX_CLIENT_CMDS; i++)
        {
            destination[i] = commandNamesArrayPtr + ((uint)i * (MAX_CMD_LENGTH + 1));
        }
    }

    public uint GetCommandAt(int index) => GetCommands()[index];
    public uint SetCommandAt(int index, uint value) => GetCommands()[index] = value;

    public string? GetCommandNameAt(int index)
    {
        Span<uint> commandNames = stackalloc uint[MAX_CLIENT_CMDS];
        GetCommandNames(commandNames);
        return AnsiString.Decode((byte*)commandNames[index]);
    }

    public void SetCommandNameAt(int index, string? value)
    {
        Span<uint> commandNames = stackalloc uint[MAX_CLIENT_CMDS];
        GetCommandNames(commandNames);
        using var valueAnsi = AnsiString.Encode(value);
        commandNames[index] = (uint)valueAnsi.Pointer;
    }
}