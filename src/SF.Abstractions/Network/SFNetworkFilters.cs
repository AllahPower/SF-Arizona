namespace SFSharp.Abstractions.Network;

/// <summary>
/// Synchronous incoming/outgoing packet filter. Receives the raw wire bytes copied into a managed
/// span. Return <see langword="true"/> to cancel (drop) the packet, <see langword="false"/> to let
/// it pass.
/// </summary>
/// <remarks>
/// Invoked on the RakNet hook thread, not the main game thread. Must be fast and non-blocking -
/// no async work, no locks, no allocations. The <see cref="ReadOnlySpan{T}"/> is only valid for
/// the duration of the callback; do not store it or pass it across threads.
/// </remarks>
public delegate bool SFPacketFilterCallback(int packetId, ReadOnlySpan<byte> data, int bitLength);

/// <summary>
/// Synchronous incoming/outgoing RPC filter. Receives the raw RPC payload copied into a managed
/// span. Return <see langword="true"/> to cancel (drop) the RPC, <see langword="false"/> to let
/// it pass.
/// </summary>
/// <remarks>
/// Invoked on the RakNet hook thread, not the main game thread. Must be fast and non-blocking -
/// no async work, no locks, no allocations. The <see cref="ReadOnlySpan{T}"/> is only valid for
/// the duration of the callback; do not store it or pass it across threads.
/// </remarks>
public delegate bool SFRpcFilterCallback(int rpcId, ReadOnlySpan<byte> payload, int bitLength);
