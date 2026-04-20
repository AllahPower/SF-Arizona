using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace SFSharp.Runtime.Bootstrap;

public static class SFBootstrap
{
    private static SFSynchronizationContext? _sc;
    private static readonly NetworkDispatcher _dispatcher = new();
    private static string? _hostDirectory;
    private static int _resolverInstalled;

    /// <summary>
    /// Directory of SF.Runtime.dll. Populated by <see cref="InstallHostAssemblyResolver"/>.
    /// Under hostfxr_initialize_for_runtime_config + load_assembly_and_get_function_pointer the
    /// TPA does not include the host's own dependencies, and AppContext.BaseDirectory points at
    /// the parent process (gta_sa.exe), not the SF\ subdirectory. We have to probe manually.
    /// </summary>
    public static string HostDirectory => _hostDirectory ?? AppContext.BaseDirectory;

    public static bool HasMainThreadDispatcher => _sc is not null;

    private static void InstallHostAssemblyResolver()
    {
        if (Interlocked.Exchange(ref _resolverInstalled, 1) != 0)
        {
            return;
        }

        string? location = typeof(SFBootstrap).Assembly.Location;
        string? dir = string.IsNullOrEmpty(location) ? null : Path.GetDirectoryName(location);
        _hostDirectory = dir;

        if (string.IsNullOrEmpty(dir))
        {
            SFLog.Warn("InstallHostAssemblyResolver: host directory unknown, plugin resolution may fail");
            return;
        }

        SFLog.Debug($"Host directory resolved: {dir}");

        AssemblyLoadContext.Default.Resolving += (ctx, name) =>
        {
            if (string.IsNullOrEmpty(name.Name))
            {
                return null;
            }

            if (PluginSharedAssemblyPolicy.TryResolveLoadedAssembly(name.Name, out Assembly? sharedAssembly) && sharedAssembly is not null)
            {
                SFLog.Debug($"Default ALC resolve '{name.Name}' -> existing {PluginSharedAssemblyPolicy.Describe(sharedAssembly)}");
                return sharedAssembly;
            }

            string candidate = Path.Combine(dir, name.Name + ".dll");
            if (!File.Exists(candidate))
            {
                return null;
            }

            SFLog.Debug($"Default ALC resolve '{name.Name}' -> {candidate}");
            return ctx.LoadFromAssemblyPath(candidate);
        };
    }

    public static RpcHandlerManager RpcHandlers => _dispatcher.IncomingRpcHandlers;
    public static OutgoingRpcManager OutgoingRpcHandlers => _dispatcher.OutgoingRpcHandlers;
    public static IncomingPacketManager IncomingPacketHandlers => _dispatcher.IncomingPacketHandlers;
    public static OutgoingPacketManager OutgoingPacketHandlers => _dispatcher.OutgoingPacketHandlers;
    public static IncomingAZVoiceControlManager IncomingAZVoiceControlHandlers => _dispatcher.IncomingAZVoiceControlHandlers;
    public static IncomingAZVoiceDataManager IncomingAZVoiceDataHandlers => _dispatcher.IncomingAZVoiceDataHandlers;

    public static NetworkFilterRegistry OutgoingPacketFilters { get; } = new();
    public static NetworkFilterRegistry OutgoingRpcFilters { get; } = new();
    public static NetworkFilterRegistry IncomingPacketFilters { get; } = new();
    public static NetworkFilterRegistry IncomingRpcFilters { get; } = new();

    public static void PostToMainThread(Action action)
    {
        _sc?.Post(x => ((Action)x!)(), action);
    }

    /// <summary>
    /// Drains queued <see cref="SFSynchronizationContext"/> continuations once. Intended for
    /// main-thread code that has to wait for async work whose continuations are routed back to
    /// the main thread (e.g. plugin unload waiting on cancelled modules to finish).
    /// Must only be called from the main thread — callers typically guard with
    /// <c>SynchronizationContext.Current is SFSynchronizationContext</c>.
    /// </summary>
    public static void PumpMainThreadQueue()
    {
        _sc?.ProcLoop();
    }

    public static void ProcessException(Exception ex)
    {
        SFLog.Error(ex, "Unhandled library exception");

        try
        {
            CChat.Instance.AddEntry(EntryType.Chat, $"{ex.GetType()}: {ex.Message}", null, 0xFFFFFFFF, 0);
        }
        catch (Exception chatEx)
        {
            SFLog.Warn($"ProcessException fallback skipped chat output: {chatEx.GetType().Name}: {chatEx.Message}");
        }
    }

    public static void ObserveTask(Task task, string source)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        task.ContinueWith(static (completed, state) =>
        {
            string taskSource = (string)state!;
            if (completed.IsFaulted && completed.Exception is not null)
            {
                SFLog.Error(completed.Exception.GetBaseException(), $"Unhandled task exception from {taskSource}");
                ProcessException(completed.Exception.GetBaseException());
            }
        }, source, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
    }

    public static void EnqueueIncomingRpc(int rpcId, byte[] packet, int payloadBitOffset, int payloadBitLength)
    {
        _dispatcher.EnqueueIncomingRpc(rpcId, packet, payloadBitOffset, payloadBitLength);
    }

    public static void EnqueueOutgoingRpc(int rpcId, byte[] packet, int dataBitLength)
    {
        _dispatcher.EnqueueOutgoingRpc(rpcId, packet, dataBitLength);
    }

    public static void EnqueueIncomingPacket(int packetId, byte[] data, int dataBitLength)
    {
        _dispatcher.EnqueueIncomingPacket(packetId, data, dataBitLength);
    }

    public static void EnqueueOutgoingPacket(int packetId, byte[] data, int dataBitLength)
    {
        _dispatcher.EnqueueOutgoingPacket(packetId, data, dataBitLength);
    }

    public static void EnqueueIncomingAZVoiceControl(int subId, byte[] data, int dataBitLength)
    {
        _dispatcher.EnqueueIncomingAZVoiceControl(subId, data, dataBitLength);
    }

    public static void EnqueueIncomingAZVoiceData(byte[] data, int dataBitLength)
    {
        _dispatcher.EnqueueIncomingAZVoiceData(data, dataBitLength);
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "WinMainLoop")]
    public static void WinMainLoop() => WinMainLoopCore(Program.Main);

    public static void WinMainLoopCore(Action main)
    {
        if (_sc is null)
        {
            SFLog.Debug("WinMainLoopCore first entry");
            InstallHostAssemblyResolver();
            _sc = new SFSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(_sc);
            SFMain(main);
        }

        _sc.ProcLoop();
    }

    private static async void SFMain(Action main)
    {
        try
        {
            SFLog.Debug("SFMain started");
            LogEnvironment();

            uint baseAddress = await GetSampDllBaseAddress();
            SFLog.Debug($"samp.dll loaded at 0x{baseAddress:X8}");

            ValidateEnvironment();

            _ = HookManager.IncomingRpcPacket;
            SFLog.Debug("IncomingRpc hook installed (pre-CNetGame).");

            await WhenCNetGameLoads(baseAddress);
            SFLog.Debug("CNetGame is ready");

            _dispatcher.Reset();
            SF.Chat.RegisterRpcBindings(_dispatcher.IncomingRpcHandlers);
            _dispatcher.IncomingRpcHandlers.StartAll();

            InstallNetworkHooks();
            InstallSubHooks();

            SF.Keyboard.StartLoop();
            SFLog.Debug("Keyboard loop started");

            PostToMainThread(main);
        }
        catch (Exception ex)
        {
            ProcessException(ex);
        }
    }

    private static void ValidateEnvironment()
    {
        SampVersionInfo? env = SampEnvironment.Detect();
        if (env is null)
        {
            SFLog.Warn("SampEnvironment.Detect returned null — samp.dll vanished?");
            return;
        }

        SFLog.Info($"SA-MP detected: version={env.Version} EP=0x{env.EntryPointRva:X} SizeOfImage=0x{env.SizeOfImage:X} TimeDateStamp=0x{env.TimeDateStamp:X} SizeOfCode=0x{env.SizeOfCode:X}");
        if (!env.IsSupported)
            SFLog.Warn($"Unsupported SA-MP version (EP=0x{env.EntryPointRva:X}). SFSharp targets 0.3.7-R3. Hooks and offsets may be incorrect.");

        // sampfuncs.asi
        SampfuncsInfo sf = env.Sampfuncs;
        if (!sf.IsLoaded)
            SFLog.Warn("SAMPFUNCS not detected. sampfuncs.asi is not loaded — some features may be unavailable.");
        else if (sf.IsSupported)
            SFLog.Info($"SAMPFUNCS detected: {sf.VersionString} EP=0x{sf.EntryPointRva:X} SizeOfImage=0x{sf.SizeOfImage:X}");
        else
            SFLog.Warn($"Unsupported SAMPFUNCS version: {sf.VersionString ?? "unknown"} (EP=0x{sf.EntryPointRva:X}). SFSharp targets v5.5.0 rel.22.");

        // _chat.asi
        if (ModuleResolver.IsModuleLoaded("_chat.asi"))
        {
            if (CArizonaChat.IsAvailable)
                SFLog.Info("_chat.asi detected, Arizona chat interop available.");
            else
                SFLog.Warn("_chat.asi loaded but chat functions could not be resolved — patterns may have changed.");
        }
        else
        {
            SFLog.Info("_chat.asi not loaded, Arizona chat features disabled.");
        }

        // AZVoice.asi
        if (ModuleResolver.IsModuleLoaded("AZVoice.asi"))
        {
            if (IncomingAZVoicePacketHook.IsAvailable)
                SFLog.Info("AZVoice.asi detected, hook target resolved.");
            else
                SFLog.Warn("AZVoice.asi loaded but hook pattern not found — voice packet capture will be unavailable.");
        }
        else
        {
            SFLog.Info("AZVoice.asi not loaded, voice features disabled.");
        }
    }

    private static void InstallNetworkHooks()
    {
        _ = HookManager.OutgoingRpcPacket;
        _ = HookManager.OutgoingPacket;
        _ = HookManager.IncomingPacket;
        SFLog.Debug("Network hooks installed: OutgoingRpc, OutgoingPacket, IncomingPacket.");

        if (HookManager.IncomingAZVoicePacket is not null)
            SFLog.Debug("AZVoice incoming packet hook installed.");

        if (HookManager.IncomingAZVoiceRpc is not null)
            SFLog.Debug("AZVoice incoming RPC hook installed.");
    }

    private static void InstallSubHooks()
    {
        HookManager.CDialogShow.AddSubHook(SF.Dialog);
        HookManager.CDialogHide.AddSubHook(SF.Dialog);
        HookManager.CDialogClose.AddSubHook(SF.Dialog);
        _ = OutgoingRpcHandlers.Subscribe(
            SFSharp.Abstractions.Interop.RakNet.ERpcId.DialogResponse,
            args => SF.Dialog.ObserveOutgoingDialogResponse(SFSharp.Runtime.Network.RakNet.Rpc.SampRpc.ParseDialogResponse(args)));
        HookManager.CChatAddEntry.AddSubHook(SF.Chat);
        HookManager.CInputCommandSend.AddSubHook(SF.Chat);
        HookManager.UpdateScoresPingsIps.AddSubHook(SF.Players);
        SFLog.Debug("Sub-hooks registered: Dialog, DialogResponseRpc, Chat, Input, Scoreboard.");
    }

    private static async Task<uint> GetSampDllBaseAddress()
    {
        while (true)
        {
            uint result = Win32.GetModuleHandle("samp.dll");
            if (result != 0)
            {
                return result;
            }

            await Task.Yield();
        }
    }

    private static async Task WhenCNetGameLoads(uint baseAddress)
    {
        SFLog.Debug($"Waiting for CNetGame pointer at samp.dll+0x{SampOffsets.CNetGame.Instance:X8} from base 0x{baseAddress:X8}");
        while (!ModuleResolver.IsClassReady("samp.dll", SampOffsets.CNetGame.Instance))
        {
            await Task.Yield();
        }
    }

    private static void LogEnvironment()
    {
        try
        {
            Assembly asm = typeof(Program).Assembly;
            string informational = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
            string fileVersion = asm.GetName().Version?.ToString() ?? "unknown";
            string asmLocation = string.IsNullOrEmpty(asm.Location) ? "<in-memory>" : asm.Location;

            Process process = Process.GetCurrentProcess();
            string processPath = process.MainModule?.FileName ?? "<unknown>";

            SFLog.Info("");
            SFLog.Info($"env.runtime      version={informational} fileVersion={fileVersion} assembly={asmLocation}");
            SFLog.Info($"env.clr          framework={RuntimeInformation.FrameworkDescription} runtimeId={RuntimeInformation.RuntimeIdentifier} processArch={RuntimeInformation.ProcessArchitecture}");
            SFLog.Info($"env.os           description={RuntimeInformation.OSDescription} osArch={RuntimeInformation.OSArchitecture} 64bitOs={Environment.Is64BitOperatingSystem} 64bitProc={Environment.Is64BitProcess}");
            SFLog.Info($"env.process      pid={process.Id} path={processPath} cwd={Environment.CurrentDirectory} cpuCount={Environment.ProcessorCount}");
            SFLog.Info($"env.paths        game={SFPaths.GameDirectory} assets={SFPaths.AssetsRoot} userData={SFPaths.UserDataRoot} host={SFBootstrap.HostDirectory}");
            SFLog.Info($"env.culture      current={System.Globalization.CultureInfo.CurrentCulture.Name} ui={System.Globalization.CultureInfo.CurrentUICulture.Name} tz={TimeZoneInfo.Local.Id}");
            SFLog.Info("");
        }
        catch (Exception ex)
        {
            SFLog.Error(ex, "LogEnvironment failed");
        }
    }
}
