namespace SFSharp;

/// <summary>Result of an input dialog request.</summary>
public readonly record struct SFDialogInputResult(SFDialogButton Button, string? Text);
