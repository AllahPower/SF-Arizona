#include <windows.h>

UINT InstallCallHook(UINT targetAddress, UINT stolenByteCount, UINT injectedFunctionPtr)
{
	UINT bufferPtr = (UINT)VirtualAlloc(NULL, stolenByteCount, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	for (UINT i = 0; i < stolenByteCount; i++)
	{
		*(BYTE*)(bufferPtr + i) = *(BYTE*)(targetAddress + i);
	}

	DWORD oldProtect;
	VirtualProtect((void*)targetAddress, stolenByteCount, PAGE_READWRITE, &oldProtect);
	*(BYTE*)targetAddress = 0xE8;
	*(UINT*)(targetAddress + 1) = injectedFunctionPtr - (targetAddress + 5);
	for (UINT i = 5; i < stolenByteCount; i++)
	{
		*(BYTE*)(targetAddress + i) = 0x90;
	}
	VirtualProtect((void*)targetAddress, stolenByteCount, oldProtect, NULL);

	return bufferPtr;
}

void __stdcall WinMainLoop();

BOOL WINAPI PeekMessageAFake(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg) {
	WinMainLoop();
	return PeekMessageA(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved) {
	DisableThreadLibraryCalls(hinstDLL);
	if (fdwReason == DLL_PROCESS_ATTACH) {
		InstallCallHook(0x748A57, 6, (UINT)&PeekMessageAFake);
	}
	return TRUE;
}