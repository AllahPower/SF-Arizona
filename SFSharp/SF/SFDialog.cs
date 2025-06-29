namespace SFSharp;

using DialogResultArgs = (SFDialogButton Button, int SelectedItemIndex, string? InputText);

public enum SFDialogButton
{
    Cancel,
    OK,
    None
};

public class SFDialog : ISFComponent
{
    void ISFComponent.Initialize()
    {
        var subHook = new SubHook();
        HookManager.CDialogShow.AddSubHook(subHook);
        HookManager.CDialogHide.AddSubHook(subHook);
        HookManager.CDialogClose.AddSubHook(subHook);
    }

    public Task<DialogResultArgs> Show(DialogStyle style, string title, string text, string okButton, string cancelButton)
    {
        CDialog.Instance.Show(0x83, style, title, text, okButton, cancelButton, false);
        return GetTask();
    }

    public async Task<SFDialogButton> ShowMessage(string title, string text)
    {
        var result = await Show(DialogStyle.MsgBox, title, text, OkCaption, CancelCaption);
        return result.Button;
    }

    public async Task<(SFDialogButton Button, string? Text)> ShowInput(string title, string text)
    {
        var result = await Show(DialogStyle.Input, title, text, OkCaption, CancelCaption);
        return (result.Button, result.InputText);
    }

    public async Task<(SFDialogButton Button, int SelectedIndex)> ShowList(string title, string[] items, string header = "")
    {
        var result = await Show(DialogStyle.TabListHeaders, title, $"{header}\r\n{string.Join("\r\n", items)}", OkCaption, CancelCaption);
        return (result.Button, result.SelectedItemIndex);
    }

    public static string OkCaption = "OK";
    public static string CancelCaption = "Cancel";
    
    
    private static TaskCompletionSource<(SFDialogButton Button, int SelectedItemIndex, string? InputText)>? _tcs;
    private static Task<DialogResultArgs> GetTask()
    {
        var tcs = new TaskCompletionSource<DialogResultArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        _tcs = tcs;
        return tcs.Task;
    }
    private static unsafe void SetResult(SFDialogButton button)
    {
        if (_tcs is not null)
        {
            var result = (button, CDialog.Instance.ListBox->GetSelectedIndex(-1), AnsiString.Decode(CDialog.Instance.Text));
            _tcs.SetResult(result);
            _tcs = null;
        }
    }

    private class SubHook :
        ISubHook<CDialogCloseArgs, NoRetValue>,
        ISubHook<CDialogHideArgs, NoRetValue>,
        ISubHook<CDialogShowHookArgs, NoRetValue>
    {
        NoRetValue ISubHook<CDialogCloseArgs, NoRetValue>.Process(CDialogCloseArgs args, Func<CDialogCloseArgs, NoRetValue> next)
        {
            SetResult((SFDialogButton)args.DialogButton);
            return next(args);
        }

        NoRetValue ISubHook<CDialogShowHookArgs, NoRetValue>.Process(CDialogShowHookArgs args, Func<CDialogShowHookArgs, NoRetValue> next)
        {
            SetResult(SFDialogButton.None);
            return next(args);
        }

        NoRetValue ISubHook<CDialogHideArgs, NoRetValue>.Process(CDialogHideArgs args, Func<CDialogHideArgs, NoRetValue> next)
        {
            SetResult(SFDialogButton.None);
            return next(args);
        }
    }
}