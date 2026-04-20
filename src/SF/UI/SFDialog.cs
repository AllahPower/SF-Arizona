namespace SFSharp.Runtime.Ui;

using SFSharp.Runtime.Network.RakNet.Rpc;

using DialogResultArgs = (SFDialogButton Button, int SelectedItemIndex, string? InputText);

public class SFDialog :
    ISFDialog,
    ISubHook<CDialogCloseArgs, NoRetValue>,
    ISubHook<CDialogHideArgs, NoRetValue>,
    ISubHook<CDialogShowHookArgs, NoRetValue>
{
    private const int InitialDialogId = 0x5346;

    public static string OkCaption = "OK";
    public static string CancelCaption = "Cancel";

    private static TaskCompletionSource<DialogResultArgs>? _tcs;
    private static int? _activeDialogId;
    private static int _nextDialogId = InitialDialogId;

    public Task<DialogResultArgs> Show(DialogStyle style, string title, string text, string okButton, string cancelButton)
    {
        var dialogId = AllocateDialogId();
        _activeDialogId = dialogId;
        _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

        SFLog.Debug($"Dialog.Show id={dialogId} style={style} title={title} ok={okButton} cancel={cancelButton}");
        CDialog.Instance.Show(dialogId, style, title, text, okButton, cancelButton, false);
        return _tcs.Task;
    }

    public async Task<SFDialogButton> ShowMessage(string title, string text)
    {
        var result = await Show(DialogStyle.MsgBox, title, text, OkCaption, CancelCaption);
        return result.Button;
    }

    public async Task<SFDialogInputResult> ShowInput(string title, string text)
    {
        var result = await Show(DialogStyle.Input, title, text, OkCaption, CancelCaption);
        return new(result.Button, result.InputText);
    }

    public async Task<SFDialogListResult> ShowList(string title, IEnumerable<string> items, string header = "")
    {
        var result = await Show(DialogStyle.TabListHeaders, title, $"{header}\r\n{string.Join("\r\n", items)}", OkCaption, CancelCaption);
        return new(result.Button, result.SelectedItemIndex);
    }

    private static int AllocateDialogId()
    {
        _nextDialogId++;
        if (_nextDialogId > short.MaxValue)
        {
            _nextDialogId = InitialDialogId;
        }

        return _nextDialogId;
    }

    private static void SetResult(DialogResultArgs result)
    {
        if (_tcs is null)
        {
            return;
        }

        SFLog.Debug($"Dialog result button={result.Button} selected={result.Item2} input={result.Item3 ?? "<null>"}");
        _tcs.SetResult(result);
        _tcs = null;
        _activeDialogId = null;
    }

    private static unsafe void SetResult(SFDialogButton button)
    {
        SetResult((button, CDialog.Instance.ListBox->SelectedIndex, AnsiString.Decode(CDialog.Instance.Text)));
    }

    public void ObserveOutgoingDialogResponse(DialogResponseRpc response)
    {
        SFLog.Debug($"Dialog response rpc id={response.DialogId} button={response.Button} list={response.ListboxId} input={response.Input ?? "<null>"} activeId={_activeDialogId?.ToString() ?? "<null>"}");
        if (_tcs is null || _activeDialogId != response.DialogId)
        {
            return;
        }

        SetResult(((SFDialogButton)response.Button, response.ListboxId, response.Input));
    }

    NoRetValue ISubHook<CDialogCloseArgs, NoRetValue>.Process(CDialogCloseArgs args, Func<CDialogCloseArgs, NoRetValue> next)
    {
        SFLog.Debug($"Dialog close button={args.DialogButton} activeId={_activeDialogId?.ToString() ?? "<null>"} currentId={CDialog.Instance.Id}");
        return next(args);
    }

    NoRetValue ISubHook<CDialogShowHookArgs, NoRetValue>.Process(CDialogShowHookArgs args, Func<CDialogShowHookArgs, NoRetValue> next)
    {
        SFLog.Debug($"Dialog show observed id={args.Id} style={args.Style} title={args.Caption ?? "<null>"}");
        if (_tcs is not null && _activeDialogId == args.Id)
        {
            SFLog.Debug($"Dialog show matched active dialog id={args.Id}");
        }

        return next(args);
    }

    NoRetValue ISubHook<CDialogHideArgs, NoRetValue>.Process(CDialogHideArgs args, Func<CDialogHideArgs, NoRetValue> next)
    {
        SFLog.Debug($"Dialog hide observed activeId={_activeDialogId?.ToString() ?? "<null>"} currentId={CDialog.Instance.Id}");
        return next(args);
    }
}
