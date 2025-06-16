using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp;

public unsafe struct CSharpExports {
    public int Version;
    public delegate* unmanaged[Stdcall]<void> MainLoop;
    public delegate* unmanaged[Stdcall]<byte*, void> LogToChat;
    public delegate* unmanaged[Stdcall]<byte*, void> SendToChat;
    public delegate* unmanaged[Stdcall]<short> GetLocalPlayerId;
    public delegate* unmanaged[Stdcall]<short> GetAimedPlayerId;
    public delegate* unmanaged[Stdcall]<int, byte> IsPlayerDefined;
    public delegate* unmanaged[Stdcall]<int, byte*> GetPlayerName;
    public delegate* unmanaged[Stdcall]<int, int> GetPlayerScore;
    public delegate* unmanaged[Stdcall]<int, int> GetPlayerPing;
    public delegate* unmanaged[Stdcall]<ushort, int, byte*, byte*, byte*, byte*, void> ShowDialog;
    public delegate* unmanaged[Stdcall]<delegate* unmanaged[Stdcall]<int, int, int, byte*, void>, void> RegisterDialogCallback;
    public delegate* unmanaged[Stdcall]<byte, byte> IsKeyDown;
    public delegate* unmanaged[Stdcall]<byte, byte> IsKeyPressed;
    public delegate* unmanaged[Stdcall]<byte*, delegate* unmanaged[Cdecl]<byte*, void>, void> RegisterChatCommand;
    public delegate* unmanaged[Stdcall]<byte*, void> UnregisterChatCommand;
    public delegate* unmanaged[Stdcall]<void*> GetChat;
    public delegate* unmanaged[Stdcall]<void> TakeScreenshot;
    public delegate* unmanaged[Stdcall]<uint, byte> IsDialogOpen;
    public delegate* unmanaged[Stdcall]<void> UpdateScoreAndPing;
}

public unsafe static partial class SFCore
{
    private static CSharpExports _exports;

    internal static unsafe void SendToChat(string message)
    {
        if (message.StartsWith('/'))
        {
            CInput.Instance.Send(message);
        }
        else
        {
            CLocalPlayer.Instance.Chat(message);
        }
        return;
        using var ansiString = AnsiString.Encode(message);
        _exports.SendToChat(ansiString);
    }

    internal static void LogToChat(string message)
    {
        CChat.Instance.AddEntry(EntryType.Chat, message, "", 0xFFAAAAAA, 0xFFAAAAAA);
        return;
        using var ansiString = AnsiString.Encode(message);
        _exports.LogToChat(ansiString);
    }

    internal static short GetLocalPlayerId() => _exports.GetLocalPlayerId();

    internal static short? GetAimedPlayerId()
    {
        var aimedPlayer = CLocalPlayer.Instance.WeaponsData.AimedPlayer;
        return aimedPlayer > 0 ? (short)aimedPlayer : null;
        var id = _exports.GetAimedPlayerId();
        return id >= 0 ? id : null;
    }

    internal static bool IsPlayerDefined(int playerId) => _exports.IsPlayerDefined(playerId) != 0;

    internal static string? GetPlayerName(int playerId)
    {
        if (!IsPlayerDefined(playerId)) return null;
        var playerName = _exports.GetPlayerName(playerId);
        return AnsiString.Decode(playerName);
    }

    internal static int? GetPlayerScore(int playerId)
    {
        if (!IsPlayerDefined(playerId)) return null;
        return _exports.GetPlayerScore(playerId);
    }

    internal static int? GetPlayerPing(int playerId)
    {
        if (!IsPlayerDefined(playerId)) return null;
        return _exports.GetPlayerPing(playerId);
    }

    internal static void ShowDialog(ushort dialogId, DialogStyle dialogStyle, string title, string content, string? button1, string? button2)
    {
        using var titleAnsi = AnsiString.Encode(title);
        using var contentAnsi = AnsiString.Encode(content);
        using var button1Ansi = AnsiString.Encode(button1);
        using var button2Ansi = AnsiString.Encode(button2);
        _exports.ShowDialog(dialogId, (int)dialogStyle, titleAnsi, contentAnsi, button1Ansi, button2Ansi);
    }

    internal static void RegisterDialogCallback(delegate* unmanaged[Stdcall]<int, int, int, byte*, void> callback)
    {
        _exports.RegisterDialogCallback(callback);
    }

    internal static bool IsKeyDown(VK key) => _exports.IsKeyDown((byte)key) != 0;

    internal static bool IsKeyPressed(VK key) => _exports.IsKeyPressed((byte)key) != 0;

    internal static void RegisterChatCommand(string command, delegate* unmanaged[Cdecl]<byte*, void> callback)
    {
        using var commandAnsi = AnsiString.Encode(command);
        _exports.RegisterChatCommand(commandAnsi, callback);
    }

    internal static void UnregisterChatCommand(string command)
    {
        using var commandAnsi = AnsiString.Encode(command);
        _exports.UnregisterChatCommand(commandAnsi);
    }

    internal static void TakeScreenshot() => _exports.TakeScreenshot();

    internal static bool IsDialogOpen(uint dialogId) => _exports.IsDialogOpen(dialogId) != 0;

    internal static void UpdateScoreAndPing() => _exports.UpdateScoreAndPing();
}

public enum DialogStyle
{
    MsgBox = 0,
    Input = 1,
    List = 2,
    Password = 3,
    TabList = 4,
    TabListHeaders = 5,
}