using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp;

public unsafe struct CSharpExports {
    public delegate* unmanaged[Stdcall]<delegate* unmanaged[Stdcall]<int, int, int, byte*, void>, void> RegisterDialogCallback;
    public delegate* unmanaged[Stdcall]<byte, byte> IsKeyDown;
    public delegate* unmanaged[Stdcall]<byte, byte> IsKeyPressed;
    public delegate* unmanaged[Stdcall]<byte*, delegate* unmanaged[Cdecl]<byte*, void>, void> RegisterChatCommand;
    public delegate* unmanaged[Stdcall]<byte*, void> UnregisterChatCommand;
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
    }

    internal static void LogToChat(string message)
    {
        CChat.Instance.AddEntry(EntryType.Chat, message, "", 0xFFAAAAAA, 0xFFAAAAAA);
    }

    internal static ushort GetLocalPlayerId()
    {
        return CPlayerPool.Instance.LocalPlayerInfo.Id;
    }

    internal static ushort? GetAimedPlayerId()
    {
        var aimedPlayer = CLocalPlayer.Instance.WeaponsData.AimedPlayer;
        return aimedPlayer != ushort.MaxValue ? aimedPlayer : null;
    }

    internal static string? GetPlayerName(ushort playerId)
    {
        return CPlayerPool.Instance.GetName(playerId);
    }

    internal static int? GetPlayerScore(ushort playerId)
    {
        return CPlayerPool.Instance.GetScore(playerId);
    }

    internal static void ShowDialog(ushort dialogId, DialogStyle dialogStyle, string title, string content, string button1, string button2)
    {
        CDialog.Instance.Show(dialogId, dialogStyle, title, content, button1, button2, false);
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

