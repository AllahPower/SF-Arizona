#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shellapi.h>
#include <stdio.h>
#include <string.h>
#include <wchar.h>

#include <coreclr_delegates.h>
#include <hostfxr.h>
#include <nethost.h>

#include "sf_native/hostfxr_bootstrap.hpp"
#include "sf_native/log.hpp"

namespace
{
constexpr const char *kRuntimeDownloadUrl = "https://dotnet.microsoft.com/en-us/download/dotnet/10.0";

enum class RuntimeFailureKind
{
    None,
    MissingRuntime,
    WrongArchitecture,
    MissingAspNetCoreFramework,
    InvalidRuntimeConfig,
    ExistingIncompatibleRuntime,
    Unknown,
};

struct RuntimeInitResult
{
    hostfxr_handle Context = nullptr;
    int Rc = 0;
    RuntimeFailureKind FailureKind = RuntimeFailureKind::None;
    bool ShouldOpenDownloadPage = false;
    bool RuntimeAlreadyInitialized = false;
    bool DifferentRuntimeProperties = false;
    char HostfxrError[4096] = {};
};

HMODULE g_hostfxrModule = nullptr;
hostfxr_initialize_for_runtime_config_fn g_initializeForRuntimeConfig = nullptr;
hostfxr_get_runtime_delegate_fn g_getRuntimeDelegate = nullptr;
hostfxr_close_fn g_closeHostfxr = nullptr;
hostfxr_set_error_writer_fn g_setErrorWriter = nullptr;
RuntimeFailureKind g_lastHostfxrLoadFailure = RuntimeFailureKind::None;

thread_local char *g_errorCaptureBuffer = nullptr;
thread_local size_t g_errorCaptureBufferSize = 0;

constexpr int kHostIncompatibleConfig = 0x800080A4u;
constexpr int kSuccessHostAlreadyInitialized = 0x00000001u;
constexpr int kSuccessDifferentRuntimeProperties = 0x00000002u;

bool ContainsInsensitive(const char *text, const char *needle)
{
    if (text == nullptr || needle == nullptr || *text == '\0' || *needle == '\0')
    {
        return false;
    }

    char haystack[4096];
    char loweredNeedle[256];
    strcpy_s(haystack, text);
    strcpy_s(loweredNeedle, needle);
    _strlwr_s(haystack);
    _strlwr_s(loweredNeedle);
    return strstr(haystack, loweredNeedle) != nullptr;
}

void HOSTFXR_CALLTYPE CaptureHostfxrError(const char_t *message)
{
    if (g_errorCaptureBuffer == nullptr || g_errorCaptureBufferSize == 0 || message == nullptr)
    {
        return;
    }

    char narrow[1024];
    int converted = WideCharToMultiByte(CP_UTF8, 0, message, -1, narrow, static_cast<int>(sizeof(narrow)), nullptr, nullptr);
    if (converted <= 0)
    {
        return;
    }

    size_t currentLength = strlen(g_errorCaptureBuffer);
    if (currentLength >= g_errorCaptureBufferSize - 1)
    {
        return;
    }

    if (currentLength != 0)
    {
        strcat_s(g_errorCaptureBuffer, g_errorCaptureBufferSize, "\n");
    }

    strcat_s(g_errorCaptureBuffer, g_errorCaptureBufferSize, narrow);
}

const char *FailureKindName(RuntimeFailureKind kind)
{
    switch (kind)
    {
    case RuntimeFailureKind::MissingRuntime:
        return "missing-runtime";
    case RuntimeFailureKind::WrongArchitecture:
        return "wrong-architecture";
    case RuntimeFailureKind::MissingAspNetCoreFramework:
        return "missing-aspnet-shared-framework";
    case RuntimeFailureKind::InvalidRuntimeConfig:
        return "invalid-runtimeconfig";
    case RuntimeFailureKind::ExistingIncompatibleRuntime:
        return "existing-incompatible-runtime";
    case RuntimeFailureKind::Unknown:
        return "unknown";
    case RuntimeFailureKind::None:
    default:
        return "none";
    }
}

RuntimeFailureKind ClassifyRuntimeFailure(int rc, const char *hostfxrError)
{
    if (rc == kHostIncompatibleConfig ||
        ContainsInsensitive(hostfxrError, "host_incompatible_config") ||
        ContainsInsensitive(hostfxrError, "already initialized") ||
        ContainsInsensitive(hostfxrError, "incompatible with"))
    {
        return RuntimeFailureKind::ExistingIncompatibleRuntime;
    }

    if (ContainsInsensitive(hostfxrError, "microsoft.aspnetcore.app"))
    {
        return RuntimeFailureKind::MissingAspNetCoreFramework;
    }

    if (ContainsInsensitive(hostfxrError, "runtimeconfig.json") &&
        (ContainsInsensitive(hostfxrError, "invalid") ||
         ContainsInsensitive(hostfxrError, "parse") ||
         ContainsInsensitive(hostfxrError, "malformed") ||
         ContainsInsensitive(hostfxrError, "error reading")))
    {
        return RuntimeFailureKind::InvalidRuntimeConfig;
    }

    if (ContainsInsensitive(hostfxrError, "architecture") ||
        ContainsInsensitive(hostfxrError, "x86") ||
        ContainsInsensitive(hostfxrError, "x64") ||
        ContainsInsensitive(hostfxrError, "arm64") ||
        ContainsInsensitive(hostfxrError, "bad image"))
    {
        return RuntimeFailureKind::WrongArchitecture;
    }

    if (ContainsInsensitive(hostfxrError, "framework") ||
        ContainsInsensitive(hostfxrError, "runtime") ||
        ContainsInsensitive(hostfxrError, "install"))
    {
        return RuntimeFailureKind::MissingRuntime;
    }

    return RuntimeFailureKind::Unknown;
}

void BuildFailureMessage(RuntimeFailureKind kind, char *outMessage, size_t outMessageSize)
{
    switch (kind)
    {
    case RuntimeFailureKind::MissingRuntime:
        strcpy_s(
            outMessage,
            outMessageSize,
            "SF-Arizona requires .NET 10 ASP.NET Core Runtime (x86).\n\n"
            "The required .NET runtime was not found.\n"
            "The official download page will be opened now.");
        break;
    case RuntimeFailureKind::WrongArchitecture:
        strcpy_s(
            outMessage,
            outMessageSize,
            "SF-Arizona requires the x86 build of .NET 10 ASP.NET Core Runtime.\n\n"
            "A .NET runtime was found, but its architecture is incompatible with this loader.\n"
            "Install the x86 runtime from the official download page.");
        break;
    case RuntimeFailureKind::MissingAspNetCoreFramework:
        strcpy_s(
            outMessage,
            outMessageSize,
            "SF-Arizona requires .NET 10 ASP.NET Core Runtime (x86).\n\n"
            "A .NET installation was found, but the ASP.NET Core shared framework is missing.\n"
            "The official download page will be opened now.");
        break;
    case RuntimeFailureKind::InvalidRuntimeConfig:
        strcpy_s(
            outMessage,
            outMessageSize,
            "SF-Arizona could not start because SF.Runtime.runtimeconfig.json is invalid or unreadable.\n\n"
            "Reinstall or update the loader files and try again.");
        break;
    case RuntimeFailureKind::ExistingIncompatibleRuntime:
        strcpy_s(
            outMessage,
            outMessageSize,
            "SF-Arizona could not start because another .NET runtime or loader is already active in this game process\n"
            "with incompatible runtime settings.\n\n"
            "Disable the conflicting .NET-based mod/loader and restart the game.");
        break;
    case RuntimeFailureKind::Unknown:
    case RuntimeFailureKind::None:
    default:
        strcpy_s(
            outMessage,
            outMessageSize,
            "SF-Arizona failed to initialize the .NET host.\n\n"
            "See sf_loader.log for the detailed hostfxr error.");
        break;
    }
}

bool OpenRuntimeDownloadPage()
{
    HINSTANCE result = ShellExecuteA(nullptr, "open", kRuntimeDownloadUrl, nullptr, nullptr, SW_SHOWNORMAL);
    if (reinterpret_cast<INT_PTR>(result) <= 32)
    {
        sf::log::Error("ShellExecuteA(download page) failed");
        return false;
    }

    sf::log::Info("opened .NET download page");
    return true;
}

void ShowRuntimeFailureMessage(const RuntimeInitResult &result)
{
    char message[1024];
    BuildFailureMessage(result.FailureKind, message, sizeof(message));
    MessageBoxA(nullptr, message, "SF-Arizona", MB_OK | MB_ICONERROR | MB_TOPMOST | MB_SETFOREGROUND);
    if (result.ShouldOpenDownloadPage)
    {
        OpenRuntimeDownloadPage();
    }
}

bool EnsureHostfxrLoaded()
{
    g_lastHostfxrLoadFailure = RuntimeFailureKind::None;
    if (g_initializeForRuntimeConfig != nullptr && g_getRuntimeDelegate != nullptr && g_closeHostfxr != nullptr)
    {
        return true;
    }

    wchar_t hostfxrPath[MAX_PATH];
    size_t bufferSize = MAX_PATH;
    int rc = get_hostfxr_path(hostfxrPath, &bufferSize, nullptr);
    if (rc != 0)
    {
        g_lastHostfxrLoadFailure = RuntimeFailureKind::MissingRuntime;
        sf::log::ErrorCode("get_hostfxr_path failed", rc);
        return false;
    }

    sf::log::InfoW("get_hostfxr_path ->", hostfxrPath);

    g_hostfxrModule = LoadLibraryW(hostfxrPath);
    if (g_hostfxrModule == nullptr)
    {
        DWORD loadError = GetLastError();
        g_lastHostfxrLoadFailure = loadError == ERROR_BAD_EXE_FORMAT
            ? RuntimeFailureKind::WrongArchitecture
            : RuntimeFailureKind::MissingRuntime;
        sf::log::Error("LoadLibraryW(hostfxr) failed");
        return false;
    }

    g_initializeForRuntimeConfig = reinterpret_cast<hostfxr_initialize_for_runtime_config_fn>(
        GetProcAddress(g_hostfxrModule, "hostfxr_initialize_for_runtime_config"));
    g_getRuntimeDelegate = reinterpret_cast<hostfxr_get_runtime_delegate_fn>(
        GetProcAddress(g_hostfxrModule, "hostfxr_get_runtime_delegate"));
    g_closeHostfxr = reinterpret_cast<hostfxr_close_fn>(GetProcAddress(g_hostfxrModule, "hostfxr_close"));
    g_setErrorWriter = reinterpret_cast<hostfxr_set_error_writer_fn>(GetProcAddress(g_hostfxrModule, "hostfxr_set_error_writer"));

    if (g_initializeForRuntimeConfig == nullptr || g_getRuntimeDelegate == nullptr || g_closeHostfxr == nullptr || g_setErrorWriter == nullptr)
    {
        g_lastHostfxrLoadFailure = RuntimeFailureKind::Unknown;
        sf::log::Error("hostfxr exports not resolved");
        return false;
    }

    sf::log::Info("hostfxr exports resolved");
    return true;
}

bool IsSuccessfulInitialization(const RuntimeInitResult &result)
{
    return result.Context != nullptr &&
           (result.Rc == 0 || result.Rc == kSuccessHostAlreadyInitialized || result.Rc == kSuccessDifferentRuntimeProperties);
}

RuntimeInitResult TryInitializeRuntime(const wchar_t *runtimeConfigPath)
{
    RuntimeInitResult result;
    g_errorCaptureBuffer = result.HostfxrError;
    g_errorCaptureBufferSize = sizeof(result.HostfxrError);
    hostfxr_error_writer_fn previousWriter = g_setErrorWriter(CaptureHostfxrError);
    result.Rc = g_initializeForRuntimeConfig(runtimeConfigPath, nullptr, &result.Context);
    g_setErrorWriter(previousWriter);
    g_errorCaptureBuffer = nullptr;
    g_errorCaptureBufferSize = 0;

    result.RuntimeAlreadyInitialized = result.Rc == kSuccessHostAlreadyInitialized || result.Rc == kSuccessDifferentRuntimeProperties;
    result.DifferentRuntimeProperties = result.Rc == kSuccessDifferentRuntimeProperties;

    if (!IsSuccessfulInitialization(result))
    {
        result.FailureKind = ClassifyRuntimeFailure(result.Rc, result.HostfxrError);
        result.ShouldOpenDownloadPage =
            result.FailureKind == RuntimeFailureKind::MissingRuntime ||
            result.FailureKind == RuntimeFailureKind::WrongArchitecture ||
            result.FailureKind == RuntimeFailureKind::MissingAspNetCoreFramework;
    }

    return result;
}

RuntimeInitResult MakeEarlyHostfxrFailure(RuntimeFailureKind kind, bool shouldOpenDownloadPage)
{
    RuntimeInitResult result;
    result.Rc = -1;
    result.FailureKind = kind;
    result.ShouldOpenDownloadPage = shouldOpenDownloadPage;
    return result;
}

void LogRuntimeInitFailure(const RuntimeInitResult &result)
{
    char line[256];
    _snprintf_s(
        line,
        sizeof(line),
        _TRUNCATE,
        "hostfxr initialize failed category=%s hr=0x%08X",
        FailureKindName(result.FailureKind),
        static_cast<unsigned>(result.Rc));
    sf::log::Error(line);

    if (result.HostfxrError[0] != '\0')
    {
        sf::log::Error("hostfxr diagnostic follows");
        sf::log::Error(result.HostfxrError);
    }
}
}

namespace sf::runtime
{
bool ValidateRuntimeAvailable(const wchar_t *runtimeConfigPath, bool interactive)
{
    if (!EnsureHostfxrLoaded())
    {
        if (interactive)
        {
            RuntimeFailureKind kind = g_lastHostfxrLoadFailure == RuntimeFailureKind::None
                ? RuntimeFailureKind::MissingRuntime
                : g_lastHostfxrLoadFailure;
            RuntimeInitResult result = MakeEarlyHostfxrFailure(
                kind,
                kind == RuntimeFailureKind::MissingRuntime || kind == RuntimeFailureKind::WrongArchitecture);
            ShowRuntimeFailureMessage(result);
        }

        return false;
    }

    RuntimeInitResult result = TryInitializeRuntime(runtimeConfigPath);
    if (IsSuccessfulInitialization(result))
    {
        if (result.Context != nullptr)
        {
            g_closeHostfxr(result.Context);
        }

        if (result.RuntimeAlreadyInitialized)
        {
            if (result.DifferentRuntimeProperties)
            {
                sf::log::Info("required .NET runtime resolved via already initialized host context (different runtime properties)");
            }
            else
            {
                sf::log::Info("required .NET runtime resolved via already initialized host context");
            }
        }

        sf::log::Info("required .NET runtime resolved");
        return true;
    }

    LogRuntimeInitFailure(result);
    if (result.Context != nullptr)
    {
        g_closeHostfxr(result.Context);
    }

    if (interactive)
    {
        ShowRuntimeFailureMessage(result);
    }

    return false;
}

bool BootstrapManagedEntry(const wchar_t *runtimeConfigPath, const wchar_t *assemblyPath, WinMainLoopFn *outEntry)
{
    if (outEntry == nullptr)
    {
        return false;
    }

    *outEntry = nullptr;

    if (!EnsureHostfxrLoaded())
    {
        return false;
    }

    RuntimeInitResult result = TryInitializeRuntime(runtimeConfigPath);
    if (!IsSuccessfulInitialization(result))
    {
        LogRuntimeInitFailure(result);
        if (result.Context != nullptr)
        {
            g_closeHostfxr(result.Context);
        }

        return false;
    }

    load_assembly_and_get_function_pointer_fn loadAssembly = nullptr;
    int rc = g_getRuntimeDelegate(result.Context, hdt_load_assembly_and_get_function_pointer, reinterpret_cast<void **>(&loadAssembly));
    g_closeHostfxr(result.Context);

    if (rc != 0 || loadAssembly == nullptr)
    {
        sf::log::ErrorCode("hostfxr_get_runtime_delegate failed", rc);
        return false;
    }

    rc = loadAssembly(
        assemblyPath,
        L"SFSharp.SFBootstrap, SF.Runtime",
        L"WinMainLoop",
        UNMANAGEDCALLERSONLY_METHOD,
        nullptr,
        reinterpret_cast<void **>(outEntry));
    if (rc != 0 || *outEntry == nullptr)
    {
        sf::log::ErrorCode("load_assembly_and_get_function_pointer(WinMainLoop) failed", rc);
        return false;
    }

    sf::log::Info("managed WinMainLoop resolved");
    return true;
}
}
