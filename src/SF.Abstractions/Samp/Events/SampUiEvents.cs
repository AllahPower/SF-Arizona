using System.Numerics;

namespace SFSharp.Abstractions.Samp.Events;

public readonly record struct SFSampTextDrawSelectionEvent(bool Enabled, uint HoverColor, ushort HoveredId);
public enum SFSampPickupChange : byte { Created, Deleting }
/// <summary>Server pickup ID and its live pool slot, model, type and position at the observation boundary.</summary>
public readonly record struct SFSampPickupEvent(int ServerId, int PoolIndex, int Model, int Type, Vector3 Position, SFSampPickupChange Change);
public readonly record struct SFSampPickupNotificationEvent(int LookupId, int PoolIndex, int? ServerId);
public enum SFSampSpectatingTarget : byte { None, Player, Vehicle }
public readonly record struct SFSampSpectatingEvent(bool Enabled, SFSampSpectatingTarget Target, int TargetId, byte CameraMode);
