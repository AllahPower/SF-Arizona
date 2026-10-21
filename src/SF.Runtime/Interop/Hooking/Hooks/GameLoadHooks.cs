using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Hooking.Hooks;

internal readonly record struct GameInitArgs(nint FileName);

/// <summary>
/// Wraps one cdecl CGame start-up function and reports the stages before and after it. The native signature
/// is <c>bool(const char*)</c>; the no-argument core-data stage has its own ABI below.
/// </summary>
internal sealed class GameInitStageHook : NativeHook<GameInitArgs, byte, GameInitStageHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte Native(nint fileName);

    private readonly SFGameLoading _loading;
    private readonly SFGameLoadStage _before;
    private readonly SFGameLoadStage _after;

    public GameInitStageHook(nint target, SFGameLoading loading, SFGameLoadStage before, SFGameLoadStage after)
    {
        _loading = loading;
        _before = before;
        _after = after;
        InstallHook(target, new Native(HookProc));
    }

    private byte HookProc(nint fileName)
    {
        GameLoadHooks.ReachSafely(_loading, _before);
        byte result = ProcessNative(new(fileName), 0);
        GameLoadHooks.ReachSafely(_loading, _after);
        return result;
    }

    protected override byte InvokeOriginalFunction(GameInitArgs args) => OriginalFunction(args.FileName);
}

/// <summary>CGame::InitialiseCoreDataAfterRW: cdecl, no arguments, boolean return in AL.</summary>
internal sealed class GameCoreDataHook : NativeHook<int, byte, GameCoreDataHook.Native>
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate byte Native();

    private readonly SFGameLoading _loading;

    public GameCoreDataHook(nint target, SFGameLoading loading)
    {
        _loading = loading;
        InstallHook(target, new Native(HookProc));
    }

    private byte HookProc()
    {
        GameLoadHooks.ReachSafely(_loading, SFGameLoadStage.BeforeCoreData);
        byte result = ProcessNative(0, 0);
        GameLoadHooks.ReachSafely(_loading, SFGameLoadStage.AfterCoreData);
        return result;
    }

    protected override byte InvokeOriginalFunction(int args) => OriginalFunction();
}

internal static unsafe class GameLoadHooks
{
    internal static void ReachSafely(SFGameLoading loading, SFGameLoadStage stage)
    {
        try { loading.Reach(stage); }
        catch (Exception ex) { HookRuntime.ReportSafely(ex); }
    }
    private readonly record struct Target(string Name, int Rva, byte[] Prologue, SFGameLoadStage Before, SFGameLoadStage After);

    // Prologues read from gta_sa.exe 1.0 US in IDA; a leading E9 means another module already detoured the entry.
    private static readonly Target[] Targets =
    [
        new("CGame::InitialiseCoreDataAfterRW", GtaOffsets.GameLoadRva.InitialiseCoreDataAfterRW, [0xE8, 0x2B, 0xAE, 0xFF, 0xFF], SFGameLoadStage.BeforeCoreData, SFGameLoadStage.AfterCoreData),
        new("CGame::Init1", GtaOffsets.GameLoadRva.Init1, [0xE8, 0x4B, 0xB4, 0xFD, 0xFF], SFGameLoadStage.BeforeInit1, SFGameLoadStage.AfterInit1),
        new("CGame::Init2", GtaOffsets.GameLoadRva.Init2, [0x56, 0x6A, 0x00, 0x68, 0xA0], SFGameLoadStage.BeforeInit2, SFGameLoadStage.AfterInit2),
        new("CGame::Init3", GtaOffsets.GameLoadRva.Init3, [0x6A, 0x00, 0x68, 0xC8, 0xA5], SFGameLoadStage.BeforeInit3, SFGameLoadStage.AfterInit3),
    ];

    private const byte JmpRel32 = 0xE9;

    public static int GameState => *(int*)ModuleResolver.GetGameAddress(GtaOffsets.GameLoadRva.GameState);

    /// <summary>Installs every stage hook whose entry is either untouched or a plain relative jump.</summary>
    public static List<IDisposable> Install(SFGameLoading loading)
    {
        List<IDisposable> hooks = new(Targets.Length);
        foreach (Target target in Targets)
        {
            nint address = ModuleResolver.GetGameAddress(target.Rva);
            ReadOnlySpan<byte> prologue = new((void*)address, target.Prologue.Length);
            string bytes = Convert.ToHexString(prologue);
            if (!prologue.SequenceEqual(target.Prologue) && prologue[0] != JmpRel32)
            {
                SFLog.Warn($"Game load hook {target.Name} skipped: unexpected prologue {bytes} at 0x{address:X8}, stages {target.Before}/{target.After} will be skipped");
                continue;
            }

            if (prologue[0] == JmpRel32 && target.Prologue[0] != JmpRel32)
            {
                SFLog.Warn($"Game load hook {target.Name} chains onto an existing detour at 0x{address:X8} ({bytes})");
            }

            hooks.Add(target.Before == SFGameLoadStage.BeforeCoreData
                ? new GameCoreDataHook(address, loading)
                : new GameInitStageHook(address, loading, target.Before, target.After));
        }

        SFLog.Debug($"Game load hooks installed: {hooks.Count}/{Targets.Length}, gGameState={GameState}");
        return hooks;
    }
}
