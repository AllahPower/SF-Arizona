using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp;

public unsafe static partial class SFCore
{
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
        CChat.Instance.AddEntry(EntryType.Debug, message, null, 0xFFAAAAAA, 0xFFAAAAAA);
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

    private class DialogCallbackHook : ISubHook<CDialogCloseArgs, NoRetValue>
    {
        public required delegate* unmanaged[Stdcall]<int, int, int, byte*, void> Callback;
        public NoRetValue Process(CDialogCloseArgs args, Func<CDialogCloseArgs, NoRetValue> next)
        {
            var cDialog = CDialog.Instance;

            var id = (int)cDialog.Id;
            var button = args.DialogButton;
            var selectedIndex = cDialog.ListBox->GetSelectedIndex(-1);
            var text = cDialog.Text;
            
            Callback(id, button, selectedIndex, text);

            return next(args);
        }
    }

    internal static void RegisterDialogCallback(delegate* unmanaged[Stdcall]<int, int, int, byte*, void> callback)
    {
        HookManager.CDialogClose.AddSubHook(new DialogCallbackHook { Callback = callback });
    }

    internal static bool IsKeyDown(VK key)
    {
        return false;
    }

    internal static bool IsKeyPressed(VK key)
    {
        return false;
    }

    internal static void RegisterChatCommand(string command, delegate* unmanaged[Cdecl]<byte*, void> callback)
    {
        CInput.Instance.AddCommand(command, callback);
    }

    internal static void UnregisterChatCommand(string command, delegate* unmanaged[Cdecl]<byte*, void> callback = null)
    {
        var commands = CInput.Instance.Commands;
        int commandCount = commands.CommandCount;

        var commandIndex = -1;
        for (int i = 0; i < commandCount; i++)
        {
            if (commands.GetCommandAt(i) == (uint)callback)
            {
                commandIndex = i;
                break;
            }
        }
        if (commandIndex == -1) throw new ArgumentException();

        //if (commands.GetCommandNameAt(commandIndex) != command) throw new ArgumentException();

        for (int i = commandIndex; i < commandCount - 2; i++)
        {
            commands.SetCommandAt(i, commands.GetCommandAt(i + 1));
            commands.SetCommandNameAt(i, commands.GetCommandNameAt(i + 1));
        }
    }

    internal static bool IsDialogOpen(uint dialogId) => CDialog.Instance.Id == dialogId;

    internal static void UpdateScoreAndPing()
    {
        CNetGame.Instance.UpdatePlayers();
    }
}