using System.Runtime.InteropServices;
using SFSharp.Runtime.Interop.Hooking.Hooks;
using NativeStub = SF.Network.Tests.SampActionAbiTests.NativeStub;

namespace SF.Network.Tests;

public sealed unsafe class SampObservationAbiTests
{
    [Fact]
    public void ObservationDelegatesPreserveEveryX86StackSlot()
    {
        using var zero = new NativeStub([0xFF, 0x01, 0xC3]);
        using var one = new NativeStub([0x8B, 0x44, 0x24, 0x04, 0x89, 0x01, 0xC2, 0x04, 0x00]);
        using var two = new NativeStub([
            0x8B, 0x44, 0x24, 0x04, 0x89, 0x01,
            0x8B, 0x44, 0x24, 0x08, 0x89, 0x41, 0x04, 0xC2, 0x08, 0x00]);
        var simple = Marshal.GetDelegateForFunctionPointer<SampObserveVoidHook.Native>(zero.Address);
        var pointer = Marshal.GetDelegateForFunctionPointer<SampObservePointerHook.Native>(one.Address);
        var integer = Marshal.GetDelegateForFunctionPointer<SampObserveIntHook.Native>(one.Address);
        var id = Marshal.GetDelegateForFunctionPointer<SampObserveIdHook.Native>(one.Address);
        var sync = Marshal.GetDelegateForFunctionPointer<SampObserveTimedSyncHook.Native>(two.Address);
        var entry = Marshal.GetDelegateForFunctionPointer<SampObserveEntryHook.Native>(two.Address);
        var pickup = Marshal.GetDelegateForFunctionPointer<SampObservePickupHook.Native>(two.Address);
        int* data = stackalloc int[2];

        for (int i = 0; i < 1_000; i++)
        {
            data[0] = 0;
            simple((nint)data);
            Assert.Equal(1, data[0]);
            pointer((nint)data, 0x12345678);
            Assert.Equal(0x12345678, data[0]);
            integer((nint)data, -7);
            Assert.Equal(-7, data[0]);
            id((nint)data, 65535);
            Assert.Equal(65535, data[0]);
            sync((nint)data, 0x12345678, 0xFEDCBA98);
            Assert.Equal(0x12345678, data[0]);
            Assert.Equal(unchecked((int)0xFEDCBA98), data[1]);
            entry((nint)data, 1999, 256);
            Assert.Equal(1999, data[0]);
            Assert.Equal(256, data[1]);
            pickup((nint)data, 0x12345678, 4095);
            Assert.Equal(0x12345678, data[0]);
            Assert.Equal(4095, data[1]);
        }
    }
}
