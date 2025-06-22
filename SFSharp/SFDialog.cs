using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp;

public record DialogResult(DialogButton Button, int SelectedItemIndex, string? InputText);

public enum DialogButton
{
    Cancel,
    Accept
};

public class DialogManager : ISubHook<CDialogCloseArgs, NoRetValue>, ISubHook<CDialogHideArgs, NoRetValue>, ISubHook<CDialogShowHookArgs, NoRetValue>
{
    public static void InstallHooks()
    {
        HookManager.CDialogShow.AddSubHook(Instance);
        HookManager.CDialogHide.AddSubHook(Instance);
        HookManager.CDialogClose.AddSubHook(Instance);
    }
    private static DialogManager Instance { get; } = new();

    private TaskCompletionSource<DialogResult?>? _tcs;

    public static Task<DialogResult?> GetTask()
    {
        var tcs = new TaskCompletionSource<DialogResult?>(TaskCreationOptions.RunContinuationsAsynchronously);
        Instance._tcs = tcs;
        return tcs.Task;
    }

    unsafe NoRetValue ISubHook<CDialogCloseArgs, NoRetValue>.Process(CDialogCloseArgs args, Func<CDialogCloseArgs, NoRetValue> next)
    {
        if (_tcs is not null)
        {
            var result = new DialogResult(
                (DialogButton)args.DialogButton,
                CDialog.Instance.ListBox->GetSelectedIndex(-1),
                AnsiString.Decode(CDialog.Instance.Text)
            );
            _tcs.SetResult(result);
            _tcs = null;
        }

        return next(args);
    }

    NoRetValue ISubHook<CDialogShowHookArgs, NoRetValue>.Process(CDialogShowHookArgs args, Func<CDialogShowHookArgs, NoRetValue> next)
    {
        if (_tcs is not null)
        {
            _tcs.SetResult(null);
            _tcs = null;
        }

        return next(args);
    }

    NoRetValue ISubHook<CDialogHideArgs, NoRetValue>.Process(CDialogHideArgs args, Func<CDialogHideArgs, NoRetValue> next)
    {
        if (_tcs is not null)
        {
            _tcs.SetResult(null);
            _tcs = null;
        }

        return next(args);
    }
}

public class SFDialog
{
    public required DialogStyle Style { get; set; }
    public required string Title { get; set; }
    public required string[] Items { get; set; }
    public string? Header { get; set; }
    public string AcceptButton { get; set; } = "OK";
    public string CancelButton { get; set; } = "";

    private const int _dialogId = 0x0083;

    public Task<DialogResult?> ShowAsync()
    {
        Show();
        return DialogManager.GetTask();
    }

    public void Show()
    {
        var content = (Header is null || Style != DialogStyle.TabListHeaders) ? string.Join("\r\n", Items) : $"{Header}\r\n{string.Join("\r\n", Items)}";
        SFCore.ShowDialog(_dialogId, Style, Title, content, AcceptButton, CancelButton);
    }
}