using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

using unsafe CDialogClose_SF = delegate* unmanaged[Cdecl]<int, int>;

namespace SFSharp;

public unsafe class CDialogCloseHook_SF : HookBase<CDialogCloseArgs, NoRetValue>, IDisposable
{
    private const int StolenBytesCount = 5;

    private static CDialogCloseHook_SF? _instance;
    private readonly uint _bufferAddress;
    private readonly uint _callAddress;
    private readonly CDialogClose_SF _functionAddress;

    public CDialogCloseHook_SF()
    {
        if (_instance is not null) throw new InvalidOperationException();

        _callAddress = HookHelper.GetFunctionPtr("sampfuncs.asi", 0x8681E);
        _functionAddress = (CDialogClose_SF)HookHelper.GetFunctionPtr("sampfuncs.asi", 0x8680F);
        _bufferAddress = HookHelper.InstallCallHook(
            _callAddress,
            StolenBytesCount,
            (uint)(CDialogClose_SF)(&HookedFunction)
        );

        _instance = this;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
    private static unsafe int HookedFunction(int dialogButton)
    {
        if (_instance is null) throw new UnreachableException();

        try
        {
            _instance.Process(new(0, (byte)dialogButton));
        }
        catch (Exception ex)
        {
            SFCore.LogException(ex);
        }
        return 0; // Unused
    }

    protected override unsafe NoRetValue InvokeOriginalFunction(CDialogCloseArgs args)
    {
        if (_instance is null) throw new UnreachableException();

        _instance._functionAddress(args.DialogButton);
        return default;
    }

    public void Dispose()
    {
        if (_instance is null) throw new InvalidOperationException();

        HookHelper.RemoveCallHook(_callAddress, StolenBytesCount, _bufferAddress);
        _instance = null;
    }
}