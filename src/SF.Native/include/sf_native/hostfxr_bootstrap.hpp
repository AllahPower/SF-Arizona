#pragma once

#include <windows.h>

namespace sf::runtime
{
	using WinMainLoopFn = void(__stdcall*)(void);

	bool ValidateRuntimeAvailable(const wchar_t* runtimeConfigPath, bool interactive);
	bool BootstrapManagedEntry(const wchar_t* runtimeConfigPath, const wchar_t* assemblyPath, WinMainLoopFn* outEntry);
}
