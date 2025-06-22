using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SFSharp;

public unsafe class CDialogCloseHook_SF : Hook<CDialogCloseArgs, NoRetValue>, IDisposable
{
    private const int StolenBytesCount = 5;

    private static CDialogCloseHook_SF? _instance;
    private readonly uint _bufferAddress;
    private readonly uint _callAddress;
    private readonly delegate* unmanaged[Cdecl]<int, int> _functionAddress;

    public CDialogCloseHook_SF() : base(BaseFunction)
    {
        if (_instance is not null) throw new InvalidOperationException();

        _callAddress = HookHelper.GetFunctionPtr("sampfuncs.asi", 0x8681E);
        _functionAddress = (delegate* unmanaged[Cdecl]<int, int>)HookHelper.GetFunctionPtr("sampfuncs.asi", 0x8680F);
        _bufferAddress = HookHelper.InstallCallHook(
            _callAddress,
            StolenBytesCount,
            (uint)(delegate* unmanaged[Cdecl]<int, int>)&HookedFunction
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

    private static unsafe NoRetValue BaseFunction(CDialogCloseArgs args)
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