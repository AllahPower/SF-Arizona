using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace SFSharp.Runtime.Bootstrap;

public static class SFBootstrap
{
    private static SFRuntime? _runtime;
    private static string? _hostDirectory;
    private static int _resolverInstalled;

    /// <summary>
    /// Directory of SF.Runtime.dll. Populated by <see cref="InstallHostAssemblyResolver"/>.
    /// Under hostfxr_initialize_for_runtime_config + load_assembly_and_get_function_pointer the
    /// TPA does not include the host's own dependencies, and AppContext.BaseDirectory points at
    /// the parent process (gta_sa.exe), not the SF\ subdirectory. We have to probe manually.
    /// </summary>
    public static string HostDirectory => _hostDirectory ?? AppContext.BaseDirectory;


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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)], EntryPoint = "WinMainLoop")]
    public static void WinMainLoop() => WinMainLoopCore(Program.Main);

    internal static void WinMainLoopCore(Action<SFRuntime> main)
    {
        if (_runtime is null)
        {
            SFLog.Debug("WinMainLoopCore first entry");
            InstallHostAssemblyResolver();
            _runtime = new SFRuntime();
            SynchronizationContext.SetSynchronizationContext(_runtime.Context);
            SFMain(_runtime, main);
        }

        _runtime.MainThread.Pump();
    }

    private static async void SFMain(SFRuntime runtime, Action<SFRuntime> main)
    {
        try
        {
            SFLog.Debug("SFMain started");
            ProtocolDiagnostics.Logger = SFLoggerProvider.Instance.CreateLogger("Protocol");
            LogEnvironment();

            runtime.InstallGameLoadHooks();
            try
            {
                Program.LoadModules(runtime);
            }
            catch (Exception ex)
            {
                runtime.Exceptions.Report(ex);
            }

            runtime.Loading.Reach(SFGameLoadStage.Startup);

            uint baseAddress = await GetSampDllBaseAddress();
            SFLog.Debug($"samp.dll loaded at 0x{baseAddress:X8}");
            SampBitStreamReader.NativeStringDecoder = new SampStringCompressorDecoder();

            ValidateEnvironment();

            runtime.InstallEarlyHooks();
            SFLog.Debug("IncomingRpc hook installed (pre-CNetGame).");

            await WhenCNetGameLoads(baseAddress);
            SFLog.Debug("CNetGame is ready");

            runtime.CreateServices();
            SFHost host = runtime.Host;
            host.ChatImpl.RegisterRpcBindings(runtime.Dispatcher.IncomingRpcHandlers);
            runtime.Dispatcher.IncomingRpcHandlers.StartAll();

            InstallNetworkHooks(runtime.Hooks);
            InstallSubHooks(runtime, host);

            host.KeyboardImpl.StartLoop();
            SFLog.Debug("Keyboard loop started");

            runtime.Loading.Reach(SFGameLoadStage.NetGameReady);

            runtime.MainThread.Post(() => main(runtime));
        }
        catch (Exception ex)
        {
            runtime.Exceptions.Report(ex);
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

        SFLog.Info(ModuleResolver.IsModuleLoaded("_chat.asi") ? "_chat.asi detected." : "_chat.asi not loaded.");

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

    private static void InstallNetworkHooks(HookRegistry hooks)
    {
        _ = hooks.OutgoingRpcPacket;
        _ = hooks.OutgoingPacket;
        _ = hooks.IncomingPacket;
        SFLog.Debug("Network hooks installed: OutgoingRpc, OutgoingPacket, IncomingPacket.");

        if (hooks.IncomingAZVoicePacket is not null)
            SFLog.Debug("AZVoice incoming packet hook installed.");

        if (hooks.IncomingAZVoiceRpc is not null)
            SFLog.Debug("AZVoice incoming RPC hook installed.");

        if (hooks.OutgoingAZVoiceRpc is not null)
            SFLog.Debug("AZVoice outgoing RPC hook installed.");
    }

    private static void InstallSubHooks(SFRuntime runtime, SFHost host)
    {
        // Only CDialog::Show is hooked. Its entry is free (the Arizona client hooks 0x40 bytes
        // further in), while the close entry it detours itself must stay untouched.
        runtime.Hooks.CDialogShow.AddSubHook(host.DialogImpl);
        _ = runtime.Dispatcher.IncomingRpcHandlers.Subscribe(
            SampRpcId.ShowDialog,
            args => host.DialogImpl.ObserveIncomingShowDialog(SampRpc.ParseShowDialog(args)));
        _ = runtime.Filters.OutgoingRpc.Add(
            (int)SampRpcId.DialogResponse,
            (dataPtr, bitLength) => host.DialogImpl.TryConsumeOwnDialogResponse(dataPtr, bitLength));
        _ = runtime.Dispatcher.OutgoingRpcHandlers.Subscribe(
            SampRpcId.DialogResponse,
            args => host.DialogImpl.ObserveOutgoingDialogResponse(SampRpc.ParseDialogResponse(args)));
        runtime.Hooks.CChatAddEntry.AddSubHook(host.ChatImpl);
        runtime.Hooks.CInputCommandSend.AddSubHook(host.ChatImpl);
        runtime.Hooks.UpdateScoresPingsIps.AddSubHook(host.PlayersImpl);
        SFLog.Debug("Sub-hooks registered: DialogShow, ShowDialogRpc, DialogResponseRpc, Chat, Input, Scoreboard.");
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
