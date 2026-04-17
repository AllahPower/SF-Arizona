namespace SFSharp;

/// <summary>Result of a list dialog request.</summary>
public readonly record struct SFDialogListResult(SFDialogButton Button, int SelectedIndex);
