using System.Runtime.InteropServices;
using SFSharp.Runtime.Interop.Hooking.Hooks;
using SFSharp.Runtime.Interop.Native;

namespace SF.Network.Tests;

public sealed unsafe class SampActionAbiTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(256)]
    [InlineData(-1)]
    public void SpawnUsesEntireEaxForNativeBool(int result)
    {
        Span<byte> code = stackalloc byte[6];
        code[0] = 0xB8; // mov eax, immediate; ret
        MemoryMarshal.Write(code[1..], in result);
        code[5] = 0xC3;
        using var stub = new NativeStub(code);
        var spawn = Marshal.GetDelegateForFunctionPointer<SampSpawnActionHook.Native>(stub.Address);

        for (int i = 0; i < 1_000; i++)
            Assert.Equal(result, spawn(0x1234));
    }

    [Fact]
    public void ActionDelegatesPassThisInEcxAndCleanCorrectStackSlots()
    {
        // Store ECX/stack arguments in caller-owned memory to exercise the actual x86 boundary.
        using var noArgs = new NativeStub([0xFF, 0x01, 0xC3]); // inc [ecx]; ret
        using var oneInt = new NativeStub([0x8B, 0x44, 0x24, 0x04, 0x89, 0x01, 0xC2, 0x04, 0x00]);
        using var oneByte = new NativeStub([0x0F, 0xB6, 0x44, 0x24, 0x04, 0x89, 0x01, 0xC2, 0x04, 0x00]);
        using var twoInts = new NativeStub([
            0x8B, 0x44, 0x24, 0x04, 0x89, 0x01,
            0x8B, 0x44, 0x24, 0x08, 0x89, 0x41, 0x04, 0xC2, 0x08, 0x00]);
        var simple = Marshal.GetDelegateForFunctionPointer<SampVoidActionHook.Native>(noArgs.Address);
        var integer = Marshal.GetDelegateForFunctionPointer<SampIntActionHook.Native>(oneInt.Address);
        var interior = Marshal.GetDelegateForFunctionPointer<SampInteriorActionHook.Native>(oneByte.Address);
        var vehicle = Marshal.GetDelegateForFunctionPointer<SampVehicleEntryActionHook.Native>(twoInts.Address);
        int* values = stackalloc int[2];

        for (int i = 0; i < 1_000; i++)
        {
            values[0] = 0;
            simple((nint)values);
            Assert.Equal(1, values[0]);
            integer((nint)values, -7);
            Assert.Equal(-7, values[0]);
            interior((nint)values, 255);
            Assert.Equal(255, values[0]);
            vehicle((nint)values, 1999, 256);
            Assert.Equal(1999, values[0]);
            Assert.Equal(256, values[1]);
        }
    }

    internal sealed class NativeStub : IDisposable
    {
        public nint Address { get; }

        public NativeStub(ReadOnlySpan<byte> code)
        {
            uint address = Win32.VirtualAlloc(0, (uint)code.Length,
                MemoryAllocationType.COMMIT | MemoryAllocationType.RESERVE, MemoryProtection.READWRITE);
            Assert.NotEqual(0u, address);
            Address = (nint)address;
            try
            {
                code.CopyTo(new Span<byte>((void*)Address, code.Length));
                Assert.True(Win32.VirtualProtect(address, (uint)code.Length, MemoryProtection.EXECUTE_READ, out _));
                Assert.True(Win32.FlushInstructionCache(-1, address, (uint)code.Length));
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Dispose() => Win32.VirtualFree((uint)Address, 0, MemoryAllocationType.RELEASE);
    }
}
