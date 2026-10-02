namespace SFSharp.Abstractions.Game;

/// <summary>
/// Reference to a GTA world entity by its pool handle (the same value GTA scripts use).
/// </summary>
/// <remarks>
/// The handle carries a slot generation, so a reference to a deleted entity stays invalid even after
/// its slot is reused: every <see cref="ISFEntities"/> member then reports that it does not exist.
/// </remarks>
public readonly record struct SFEntityRef(SFEntityKind Kind, int Handle);
