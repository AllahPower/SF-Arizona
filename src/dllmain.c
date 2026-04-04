#include <windows.h>

void __stdcall WinMainLoop(void);

static BOOL(WINAPI *g_OrigPeekMessageA)(LPMSG, HWND, UINT, UINT, UINT) = NULL;
static FARPROC *g_pIATEntry = NULL;

static BOOL WINAPI HookedPeekMessageA(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg)
{
    WinMainLoop();
    return g_OrigPeekMessageA(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
}

static FARPROC *FindIATEntry(HMODULE hModule, const char *dllName, const char *funcName)
{
    BYTE *base = (BYTE *)hModule;
    IMAGE_DOS_HEADER *dos = (IMAGE_DOS_HEADER *)base;
    if (dos->e_magic != IMAGE_DOS_SIGNATURE)
        return NULL;

    IMAGE_NT_HEADERS *nt = (IMAGE_NT_HEADERS *)(base + dos->e_lfanew);
    if (nt->Signature != IMAGE_NT_SIGNATURE)
        return NULL;

    DWORD importRva = nt->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT].VirtualAddress;
    if (importRva == 0)
        return NULL;

    IMAGE_IMPORT_DESCRIPTOR *imp = (IMAGE_IMPORT_DESCRIPTOR *)(base + importRva);

    for (; imp->Name; imp++)
    {
        if (_stricmp((const char *)(base + imp->Name), dllName) != 0)
            continue;

        IMAGE_THUNK_DATA *nameThunk = (IMAGE_THUNK_DATA *)(base + imp->OriginalFirstThunk);
        IMAGE_THUNK_DATA *addrThunk = (IMAGE_THUNK_DATA *)(base + imp->FirstThunk);

        for (; nameThunk->u1.AddressOfData; nameThunk++, addrThunk++)
        {
            if (nameThunk->u1.Ordinal & IMAGE_ORDINAL_FLAG)
                continue;

            IMAGE_IMPORT_BY_NAME *hint = (IMAGE_IMPORT_BY_NAME *)(base + nameThunk->u1.AddressOfData);
            if (strcmp(hint->Name, funcName) == 0)
                return (FARPROC *)&addrThunk->u1.Function;
        }
    }

    return NULL;
}

static BOOL InstallHook(void)
{
    HMODULE exe = GetModuleHandleA(NULL);
    if (!exe)
        return FALSE;

    g_pIATEntry = FindIATEntry(exe, "USER32.dll", "PeekMessageA");
    if (!g_pIATEntry)
        return FALSE;

    g_OrigPeekMessageA = (BOOL(WINAPI *)(LPMSG, HWND, UINT, UINT, UINT)) * g_pIATEntry;

    DWORD oldProtect;
    if (!VirtualProtect(g_pIATEntry, sizeof(FARPROC), PAGE_READWRITE, &oldProtect))
        return FALSE;

    *g_pIATEntry = (FARPROC)HookedPeekMessageA;
    VirtualProtect(g_pIATEntry, sizeof(FARPROC), oldProtect, &oldProtect);
    return TRUE;
}

static void RestoreHook(void)
{
    if (!g_pIATEntry || !g_OrigPeekMessageA)
        return;

    DWORD oldProtect;
    if (!VirtualProtect(g_pIATEntry, sizeof(FARPROC), PAGE_READWRITE, &oldProtect))
        return;

    *g_pIATEntry = (FARPROC)g_OrigPeekMessageA;
    VirtualProtect(g_pIATEntry, sizeof(FARPROC), oldProtect, &oldProtect);
    g_pIATEntry = NULL;
    g_OrigPeekMessageA = NULL;
}

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
    if (fdwReason == DLL_PROCESS_ATTACH)
    {
        DisableThreadLibraryCalls(hinstDLL);
        return InstallHook();
    }

    if (fdwReason == DLL_PROCESS_DETACH && lpvReserved == NULL)
    {
        RestoreHook();
    }

    return TRUE;
}
