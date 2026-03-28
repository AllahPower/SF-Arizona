namespace SFSharp;

public readonly record struct SFObjectMaterialTextInfo(
    byte MaterialIndex,
    byte MaterialSize,
    string? FontName,
    byte FontSize,
    bool Bold,
    uint FontColor,
    uint BackgroundColor,
    byte Align);
