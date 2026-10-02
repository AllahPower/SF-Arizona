namespace SFSharp.Runtime.Ui;

using DialogResultArgs = (SFDialogButton Button, int SelectedItemIndex, string? InputText);

/// <summary>
/// SA-MP dialog facade. <c>CDialog::Show</c> is hooked at its entry, which the Arizona client leaves
/// free (its own hook sits 0x40 bytes further in). <c>CDialog::Hide</c> and <c>CDialog::Close</c> are
/// deliberately left alone: the client detours the close entry itself, and its CEF path is the only
/// thing that still draws dialogs. Completion comes from outgoing RPC 62.
/// </summary>
public class SFDialog : ISFDialog, ISubHook<CDialogShowHookArgs, NoRetValue>
{
    private const int InitialDialogId = 0x5346;
    private const int AppearTimeoutMs = 5000;
    private const int ResponseGraceMs = 750;

    public static string OkCaption = "OK";
    public static string CancelCaption = "Cancel";

    private const int NoDialogId = -1;

    private static TaskCompletionSource<DialogResultArgs>? _tcs;
    private static volatile int _activeDialogId = NoDialogId;
    private static int _nextDialogId = InitialDialogId;

    /// <summary>Last dialog the server pushed through RPC 61, or <c>null</c> if none was seen yet.</summary>
    public ShowDialogRpc? LastServerDialog { get; private set; }

    /// <summary>
    /// Last dialog that reached native <c>CDialog::Show</c>, server-sent or opened by SF itself.
    /// </summary>
    public CDialogShowHookArgs? LastShownDialog { get; private set; }

    public Task<DialogResultArgs> Show(DialogStyle style, string title, string text, string okButton, string cancelButton)
    {
        AbandonPendingDialog();

        int dialogId = AllocateDialogId();
        TaskCompletionSource<DialogResultArgs> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        _activeDialogId = dialogId;
        _tcs = tcs;

        SFLog.Debug($"Dialog.Show id={dialogId} style={style} title={title} ok={okButton} cancel={cancelButton}");
        // Shown as server-side on purpose. The Arizona client only produces a DialogResponse for a
        // dialog whose m_bServerside is set; with it clear, closing the dialog yields nothing at all
        // and the caller can never learn which row was picked. The response is intercepted and
        // cancelled in TryConsumeOwnDialogResponse, so it never reaches the server.
        CDialog.Instance.Show(dialogId, style, title, text, okButton, cancelButton, true);
        SFBootstrap.ObserveTask(WatchNativeDialog(dialogId, tcs), $"{nameof(SFDialog)}.{nameof(WatchNativeDialog)}");
        return tcs.Task;
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

    public void ObserveIncomingShowDialog(ShowDialogRpc dialog)
    {
        LastServerDialog = dialog;
        SFLog.Debug($"Dialog rpc id={dialog.DialogId} style={dialog.Style} title={dialog.Title} textLength={dialog.Text.Length}");
    }

    NoRetValue ISubHook<CDialogShowHookArgs, NoRetValue>.Process(CDialogShowHookArgs args, Func<CDialogShowHookArgs, NoRetValue> next)
    {
        LastShownDialog = args;
        SFLog.Debug($"Dialog show observed id={args.Id} style={args.Style} serverSide={args.ServerSide} title={args.Caption ?? "<null>"}");
        return next(args);
    }

    public void ObserveOutgoingDialogResponse(DialogResponseRpc response)
    {
        SFLog.Debug($"Dialog response rpc id={response.DialogId} button={response.Button} list={response.ListboxId} input={response.Input ?? "<null>"} activeId={_activeDialogId}");
        if (_tcs is null || _activeDialogId != response.DialogId)
        {
            return;
        }

        SetResult(((SFDialogButton)response.Button, response.ListboxId, response.Input));
    }

    /// <summary>
    /// Outgoing RPC 62 filter. Runs on the hook thread: claims the response when it belongs to a
    /// dialog SF opened itself and cancels it, so a dialog the server never sent does not produce
    /// an unsolicited DialogResponse on the wire.
    /// </summary>
    internal bool TryConsumeOwnDialogResponse(nint dataPtr, int bitLength)
    {
        int activeId = _activeDialogId;
        if (activeId == NoDialogId || dataPtr == 0)
        {
            return false;
        }

        DialogResponseRpc response = SampRpc.ParseDialogResponse(new OutgoingRpcArgs((int)SampRpcId.DialogResponse, dataPtr, bitLength));
        if (response.DialogId != activeId)
        {
            return false;
        }

        SFLog.Debug($"Dialog response rpc claimed id={response.DialogId} button={response.Button} list={response.ListboxId} input={response.Input ?? "<null>"}");
        SFBootstrap.PostToMainThread(() => SetResult(((SFDialogButton)response.Button, response.ListboxId, response.Input)));
        return true;
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

    private static void AbandonPendingDialog()
    {
        if (_tcs is null)
        {
            return;
        }

        SFLog.Warn($"Dialog.Show replaced a pending dialog id={_activeDialogId}; completing it with {nameof(SFDialogButton.None)}");
        SetResult((SFDialogButton.None, -1, null));
    }

    private static void SetResult(DialogResultArgs result)
    {
        if (_tcs is null)
        {
            return;
        }

        SFLog.Debug($"Dialog result button={result.Button} selected={result.SelectedItemIndex} input={result.InputText ?? "<null>"}");
        TaskCompletionSource<DialogResultArgs> tcs = _tcs;
        _tcs = null;
        _activeDialogId = NoDialogId;
        tcs.SetResult(result);
    }

    /// <summary>
    /// Completes the dialog when it disappears without producing a DialogResponse RPC (ESC, or a
    /// client that closes it on its own). Runs on the main thread, one poll per pumped frame.
    /// </summary>
    private static async Task WatchNativeDialog(int dialogId, TaskCompletionSource<DialogResultArgs> tcs)
    {
        long deadline = Environment.TickCount64 + AppearTimeoutMs;
        while (!tcs.Task.IsCompleted && !IsNativeDialogActive(dialogId))
        {
            if (Environment.TickCount64 > deadline)
            {
                SFLog.Warn($"Dialog id={dialogId} never became active within {AppearTimeoutMs} ms; completing it with {nameof(SFDialogButton.None)}");
                CompleteIfCurrent(tcs, (SFDialogButton.None, -1, null));
                return;
            }

            await Task.Yield();
        }

        int selectedIndex = -1;
        while (!tcs.Task.IsCompleted && IsNativeDialogActive(dialogId))
        {
            selectedIndex = ReadSelectedIndex();
            await Task.Yield();
        }

        // The dialog is gone from native state, but that state is cleared synchronously while the
        // matching DialogResponse RPC only reaches the main thread on a later pump - and this
        // continuation is already queued ahead of it. Wait for it instead of racing it.
        long responseDeadline = Environment.TickCount64 + ResponseGraceMs;
        while (!tcs.Task.IsCompleted && Environment.TickCount64 < responseDeadline)
        {
            await Task.Yield();
        }

        if (tcs.Task.IsCompleted)
        {
            return;
        }

        SFLog.Debug($"Dialog watchdog observed close without a response rpc id={dialogId} selected={selectedIndex}");
        CompleteIfCurrent(tcs, (SFDialogButton.None, selectedIndex, null));
    }

    private static void CompleteIfCurrent(TaskCompletionSource<DialogResultArgs> tcs, DialogResultArgs result)
    {
        if (!ReferenceEquals(_tcs, tcs))
        {
            return;
        }

        SetResult(result);
    }

    private static bool IsNativeDialogActive(int dialogId)
    {
        if (!CDialog.IsAvailable)
        {
            return false;
        }

        ref readonly CDialog dialog = ref CDialog.Instance;
        return dialog.IsActive && dialog.Id == (uint)dialogId;
    }

    private static unsafe int ReadSelectedIndex()
    {
        CDXUTListBox* listBox = CDialog.Instance.ListBox;
        return listBox is null ? -1 : listBox->SelectedIndex;
    }
}
