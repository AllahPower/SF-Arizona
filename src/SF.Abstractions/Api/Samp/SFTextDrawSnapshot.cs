namespace SFSharp.Abstractions.Samp;

public readonly record struct SFTextDrawSnapshot(
    string? Text,
    string? String,
    SFTextDrawDataSnapshot Data);
