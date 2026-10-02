#pragma once

#include <windows.h>

namespace sf::hooks
{
	using TickCallback = void(__stdcall*)(void);

	bool InstallPeekMessageHook();
	void RestorePeekMessageHook();
	void SetTickCallback(TickCallback callback);

	// Releases the first PeekMessageA call, which waits for the managed bootstrap so the runtime sees the
	// game from its first loop iteration. Call it on every bootstrap exit path.
	void SignalBootstrapFinished();
}
