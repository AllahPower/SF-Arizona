namespace SFSharp.Runtime.Interop.Engine;

/// <summary>
/// GTA script handle encoding used by <c>CPools::Get*Ref</c>: <c>(slot &lt;&lt; 8) + generation</c>.
/// <c>CPools::Get*</c> accepts a handle only when its low byte equals the slot's current byte-map entry.
/// </summary>
public static class GtaPoolHandle
{
    public static int Compose(int slot, byte generation) => (slot << 8) + generation;

    public static int GetSlot(int handle) => handle >> 8;

    public static byte GetGeneration(int handle) => (byte)handle;

    public static bool IsFree(byte byteMapEntry) => (byteMapEntry & GtaOffsets.CPool.FreeSlotFlag) != 0;

    /// <summary>Range check the native getters skip; an out-of-range slot would read past the byte map.</summary>
    public static bool IsInRange(int handle, int capacity) => handle >= 0 && GetSlot(handle) < capacity;
}
