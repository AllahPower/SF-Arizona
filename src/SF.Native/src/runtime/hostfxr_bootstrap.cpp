#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <shellapi.h>
#include <stdio.h>
#include <wchar.h>

#include <coreclr_delegates.h>
#include <hostfxr.h>
#include <nethost.h>

#include "sf_native/hostfxr_bootstrap.hpp"
#include "sf_native/log.hpp"

namespace
{
constexpr const char *kRuntimeDownloadUrl = "https://dotnet.microsoft.com/en-us/download/dotnet/10.0";
constexpr const char *kMissingRuntimeMessage =
    "SF-Arizona requires .NET 10 ASP.NET Core Runtime (x86).\n\n"
    "The required runtime was not found or could not be resolved.\n"
    "The official download page will be opened now.";

HMODULE g_hostfxrModule = nullptr;
hostfxr_initialize_for_runtime_config_fn g_initializeForRuntimeConfig = nullptr;
hostfxr_get_runtime_delegate_fn g_getRuntimeDelegate = nullptr;
hostfxr_close_fn g_closeHostfxr = nullptr;

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

void ShowMissingRuntimeMessage()
{
    MessageBoxA(nullptr, kMissingRuntimeMessage, "SF-Arizona", MB_OK | MB_ICONERROR | MB_TOPMOST | MB_SETFOREGROUND);
    OpenRuntimeDownloadPage();
}

bool EnsureHostfxrLoaded()
{
    if (g_initializeForRuntimeConfig != nullptr && g_getRuntimeDelegate != nullptr && g_closeHostfxr != nullptr)
    {
        return true;
    }

    wchar_t hostfxrPath[MAX_PATH];
    size_t bufferSize = MAX_PATH;
    int rc = get_hostfxr_path(hostfxrPath, &bufferSize, nullptr);
    if (rc != 0)
    {
        sf::log::ErrorCode("get_hostfxr_path failed", rc);
        return false;
    }

    sf::log::InfoW("get_hostfxr_path ->", hostfxrPath);

    g_hostfxrModule = LoadLibraryW(hostfxrPath);
    if (g_hostfxrModule == nullptr)
    {
        sf::log::Error("LoadLibraryW(hostfxr) failed");
        return false;
    }

    g_initializeForRuntimeConfig = reinterpret_cast<hostfxr_initialize_for_runtime_config_fn>(
        GetProcAddress(g_hostfxrModule, "hostfxr_initialize_for_runtime_config"));
    g_getRuntimeDelegate = reinterpret_cast<hostfxr_get_runtime_delegate_fn>(
        GetProcAddress(g_hostfxrModule, "hostfxr_get_runtime_delegate"));
    g_closeHostfxr = reinterpret_cast<hostfxr_close_fn>(GetProcAddress(g_hostfxrModule, "hostfxr_close"));

    if (g_initializeForRuntimeConfig == nullptr || g_getRuntimeDelegate == nullptr || g_closeHostfxr == nullptr)
    {
        sf::log::Error("hostfxr exports not resolved");
        return false;
    }

    sf::log::Info("hostfxr exports resolved");
    return true;
}

bool TryInitializeRuntime(const wchar_t *runtimeConfigPath, hostfxr_handle *outHandle, int *outRc)
{
    *outHandle = nullptr;
    *outRc = g_initializeForRuntimeConfig(runtimeConfigPath, nullptr, outHandle);
    return *outRc == 0 && *outHandle != nullptr;
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
            ShowMissingRuntimeMessage();
        }

        return false;
    }

    hostfxr_handle context = nullptr;
    int rc = 0;
    if (TryInitializeRuntime(runtimeConfigPath, &context, &rc))
    {
        g_closeHostfxr(context);
        sf::log::Info("required .NET runtime resolved");
        return true;
    }

    sf::log::ErrorCode("hostfxr_initialize_for_runtime_config failed", rc);
    if (context != nullptr)
    {
        g_closeHostfxr(context);
    }

    if (interactive)
    {
        ShowMissingRuntimeMessage();
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

    hostfxr_handle context = nullptr;
    int rc = 0;
    if (!TryInitializeRuntime(runtimeConfigPath, &context, &rc))
    {
        sf::log::ErrorCode("hostfxr_initialize_for_runtime_config failed", rc);
        if (context != nullptr)
        {
            g_closeHostfxr(context);
        }

        return false;
    }

    load_assembly_and_get_function_pointer_fn loadAssembly = nullptr;
    rc = g_getRuntimeDelegate(context, hdt_load_assembly_and_get_function_pointer, reinterpret_cast<void **>(&loadAssembly));
    g_closeHostfxr(context);

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
