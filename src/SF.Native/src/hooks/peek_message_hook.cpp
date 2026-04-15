#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <string.h>

#include "sf_native/log.hpp"
#include "sf_native/peek_message_hook.hpp"

namespace
{
	BOOL(WINAPI* g_originalPeekMessageA)(LPMSG, HWND, UINT, UINT, UINT) = nullptr;
	FARPROC* g_iatEntry = nullptr;
	volatile LONG g_tickReentrancy = 0;
	sf::hooks::TickCallback g_tickCallback = nullptr;

	BOOL WINAPI HookedPeekMessageA(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg)
	{
		sf::hooks::TickCallback tick = g_tickCallback;
		if (tick != nullptr && InterlockedCompareExchange(&g_tickReentrancy, 1, 0) == 0)
		{
			__try
			{
				tick();
			}
			__except (EXCEPTION_EXECUTE_HANDLER)
			{
				sf::log::Error("SEH in managed WinMainLoop");
			}

			InterlockedExchange(&g_tickReentrancy, 0);
		}

		return g_originalPeekMessageA(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
	}

	FARPROC* FindIatEntry(HMODULE module, const char* dllName, const char* functionName)
	{
		BYTE* base = reinterpret_cast<BYTE*>(module);
		auto* dos = reinterpret_cast<IMAGE_DOS_HEADER*>(base);
		if (dos->e_magic != IMAGE_DOS_SIGNATURE)
		{
			return nullptr;
		}

		auto* nt = reinterpret_cast<IMAGE_NT_HEADERS*>(base + dos->e_lfanew);
		if (nt->Signature != IMAGE_NT_SIGNATURE)
		{
			return nullptr;
		}

		DWORD importRva = nt->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress;
		if (importRva == 0)
		{
			return nullptr;
		}

		auto* imports = reinterpret_cast<IMAGE_IMPORT_DESCRIPTOR*>(base + importRva);
		for (; imports->Name != 0; ++imports)
		{
			if (_stricmp(reinterpret_cast<const char*>(base + imports->Name), dllName) != 0)
			{
				continue;
			}

			auto* nameThunk = reinterpret_cast<IMAGE_THUNK_DATA*>(base + imports->OriginalFirstThunk);
			auto* addressThunk = reinterpret_cast<IMAGE_THUNK_DATA*>(base + imports->FirstThunk);
			for (; nameThunk->u1.AddressOfData != 0; ++nameThunk, ++addressThunk)
			{
				if ((nameThunk->u1.Ordinal & IMAGE_ORDINAL_FLAG) != 0)
				{
					continue;
				}

				auto* importByName = reinterpret_cast<IMAGE_IMPORT_BY_NAME*>(base + nameThunk->u1.AddressOfData);
				if (strcmp(reinterpret_cast<const char*>(importByName->Name), functionName) == 0)
				{
					return reinterpret_cast<FARPROC*>(&addressThunk->u1.Function);
				}
			}
		}

		return nullptr;
	}
}

namespace sf::hooks
{
	void SetTickCallback(TickCallback callback)
	{
		g_tickCallback = callback;
	}

	bool InstallPeekMessageHook()
	{
		HMODULE exeModule = GetModuleHandleA(nullptr);
		if (exeModule == nullptr)
		{
			sf::log::Error("GetModuleHandleA(NULL) failed");
			return false;
		}

		g_iatEntry = FindIatEntry(exeModule, "USER32.dll", "PeekMessageA");
		if (g_iatEntry == nullptr)
		{
			sf::log::Error("PeekMessageA IAT entry not found");
			return false;
		}

		g_originalPeekMessageA = reinterpret_cast<BOOL(WINAPI*)(LPMSG, HWND, UINT, UINT, UINT)>(*g_iatEntry);

		DWORD oldProtect = 0;
		if (!VirtualProtect(g_iatEntry, sizeof(FARPROC), PAGE_READWRITE, &oldProtect))
		{
			sf::log::Error("VirtualProtect(RW) failed");
			return false;
		}

		*g_iatEntry = reinterpret_cast<FARPROC>(HookedPeekMessageA);
		VirtualProtect(g_iatEntry, sizeof(FARPROC), oldProtect, &oldProtect);
		sf::log::Info("IAT hook installed");
		return true;
	}

	void RestorePeekMessageHook()
	{
		if (g_iatEntry == nullptr || g_originalPeekMessageA == nullptr)
		{
			return;
		}

		DWORD oldProtect = 0;
		if (!VirtualProtect(g_iatEntry, sizeof(FARPROC), PAGE_READWRITE, &oldProtect))
		{
			return;
		}

		*g_iatEntry = reinterpret_cast<FARPROC>(g_originalPeekMessageA);
		VirtualProtect(g_iatEntry, sizeof(FARPROC), oldProtect, &oldProtect);
		g_iatEntry = nullptr;
		g_originalPeekMessageA = nullptr;
		g_tickCallback = nullptr;
	}
}
