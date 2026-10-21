namespace SFSharp.Abstractions.Network;

/// <summary>Optional extension-owned labels and sub-payload boundary for a raw packet.</summary>
public sealed record SFPacketMetadata(string Name, int? SubId = null, string? Category = null, string? Group = null, int HeaderBitLength = 0);
