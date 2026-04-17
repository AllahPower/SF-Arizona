namespace SFSharp;

/// <summary>
/// Plugin-facing contract for SA-MP dialogs. Concrete implementation lives in the host and routes
/// requests through the active dialog runtime.
/// </summary>
/// <remarks>
/// NOT thread-safe. Every <c>ShowXxx</c> call drives native SA-MP dialog state and must be invoked
/// from the main game thread. Use <see cref="IModuleContext.SwitchToMainThreadAsync"/> first when
/// calling from a background context.
/// </remarks>
public interface ISFDialog
{
    /// <summary>Shows a message box style dialog and returns the pressed button.</summary>
    Task<SFDialogButton> ShowMessage(string title, string text);

    /// <summary>Shows an input dialog and returns the pressed button plus the typed text.</summary>
    Task<SFDialogInputResult> ShowInput(string title, string text);

    /// <summary>Shows a list dialog and returns the pressed button plus the selected row index.</summary>
    Task<SFDialogListResult> ShowList(string title, IEnumerable<string> items, string header = "");
}
