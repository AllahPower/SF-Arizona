#pragma once

#include <windows.h>

namespace sf::hooks
{
	using TickCallback = void(__stdcall*)(void);

	bool InstallPeekMessageHook();
	void RestorePeekMessageHook();
	void SetTickCallback(TickCallback callback);
}
