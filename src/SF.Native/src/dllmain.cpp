#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <stdio.h>
#include <wchar.h>

#include "sf_native/hostfxr_bootstrap.hpp"
#include "sf_native/log.hpp"
#include "sf_native/peek_message_hook.hpp"

namespace
{
	wchar_t g_baseDir[MAX_PATH] = {};
	constexpr const wchar_t* kManagedRootDirectoryName = L"SF";

	bool ResolveBaseDirectory(HINSTANCE instance, wchar_t* outDir, size_t outCount)
	{
		wchar_t path[MAX_PATH];
		DWORD length = GetModuleFileNameW(instance, path, MAX_PATH);
		if (length == 0 || length >= MAX_PATH)
		{
			return false;
		}

		wchar_t* lastSlash = wcsrchr(path, L'\\');
		if (lastSlash == nullptr)
		{
			return false;
		}

		*lastSlash = L'\0';
		return wcscpy_s(outDir, outCount, path) == 0;
	}

	bool ComposePath(const wchar_t* baseDir, const wchar_t* relativePath, wchar_t* outPath, size_t outCount)
	{
		return swprintf(outPath, outCount, L"%s\\%s", baseDir, relativePath) >= 0;
	}

	DWORD WINAPI BootstrapThreadProc(LPVOID)
	{
		sf::log::Info("bootstrap worker thread started");

		wchar_t managedRootPath[MAX_PATH];
		wchar_t runtimeConfigPath[MAX_PATH];
		wchar_t assemblyPath[MAX_PATH];
		if (!ComposePath(g_baseDir, kManagedRootDirectoryName, managedRootPath, MAX_PATH) ||
			!ComposePath(managedRootPath, L"SF.Runtime.runtimeconfig.json", runtimeConfigPath, MAX_PATH) ||
			!ComposePath(managedRootPath, L"SF.Runtime.dll", assemblyPath, MAX_PATH))
		{
			sf::log::Error("failed to compose managed paths");
			return 1;
		}

		sf::log::InfoW("managed root:", managedRootPath);

		if (GetFileAttributesW(runtimeConfigPath) == INVALID_FILE_ATTRIBUTES)
		{
			sf::log::ErrorW("runtimeconfig not found at", runtimeConfigPath);
			return 1;
		}

		if (GetFileAttributesW(assemblyPath) == INVALID_FILE_ATTRIBUTES)
		{
			sf::log::ErrorW("SF.Runtime.dll not found at", assemblyPath);
			return 1;
		}

		if (!sf::runtime::ValidateRuntimeAvailable(runtimeConfigPath, true))
		{
			sf::log::Error("required .NET runtime is unavailable");
			return 1;
		}

		sf::runtime::WinMainLoopFn winMainLoop = nullptr;
		if (!sf::runtime::BootstrapManagedEntry(runtimeConfigPath, assemblyPath, &winMainLoop))
		{
			sf::log::Error("managed bootstrap failed, game continues without managed tick");
			return 1;
		}

		sf::hooks::SetTickCallback(winMainLoop);
		sf::log::Info("managed bootstrap complete");
		return 0;
	}
}

BOOL WINAPI DllMain(HINSTANCE instance, DWORD reason, LPVOID reserved)
{
	if (reason == DLL_PROCESS_ATTACH)
	{
		DisableThreadLibraryCalls(instance);

		if (!ResolveBaseDirectory(instance, g_baseDir, MAX_PATH))
		{
			return TRUE;
		}

		sf::log::Open(g_baseDir);
		sf::log::Info("===== SF.asi loader starting =====");
		sf::log::InfoW("base dir:", g_baseDir);

		if (!sf::hooks::InstallPeekMessageHook())
		{
			sf::log::Error("IAT hook install failed");
			return TRUE;
		}

		HANDLE threadHandle = CreateThread(nullptr, 0, BootstrapThreadProc, nullptr, 0, nullptr);
		if (threadHandle == nullptr)
		{
			sf::log::Error("CreateThread(Bootstrap) failed");
			return TRUE;
		}

		CloseHandle(threadHandle);
		sf::log::Info("loader ready, bootstrap deferred to worker thread");
		return TRUE;
	}

	if (reason == DLL_PROCESS_DETACH && reserved == nullptr)
	{
		sf::hooks::RestorePeekMessageHook();
	}

	return TRUE;
}
