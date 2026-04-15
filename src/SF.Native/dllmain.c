#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <wchar.h>

#include <nethost.h>
#include <coreclr_delegates.h>
#include <hostfxr.h>

/* ============================================================================
 * SF.asi — native loader that boots CoreCLR via hostfxr and hands control to
 * SFSharp.SFBootstrap.WinMainLoop() on every game tick.
 *
 * Layout next to gta_sa.exe:
 *   SF.asi                         this module (x86)
 *   SF.Runtime.dll                 managed entry point
 *   SF.Runtime.runtimeconfig.json  framework: Microsoft.AspNetCore.App 10.0
 *   SF.Runtime.deps.json           managed deps graph
 *   SF.Abstractions.dll            public contracts
 *   <managed deps *.dll>           MinHook.NET, logging abstractions, ...
 *
 * Pipeline (runs once in DLL_PROCESS_ATTACH):
 *   1. LogOpen()  — text log next to the .asi
 *   2. ResolveBaseDirectory() — directory this DLL was loaded from
 *   3. get_hostfxr_path(nullptr) → .../host/fxr/<v>/hostfxr.dll
 *   4. LoadLibraryW(hostfxr_path) + GetProcAddress init/close/get_delegate
 *   5. hostfxr_initialize_for_runtime_config(L"SF.Runtime.runtimeconfig.json")
 *   6. hostfxr_get_runtime_delegate(hdt_load_assembly_and_get_function_pointer)
 *   7. loader_fn(L"SF.Runtime.dll", L"SFSharp.SFBootstrap, SF.Runtime",
 *                L"WinMainLoop", UNMANAGEDCALLERSONLY_METHOD, nullptr,
 *                &g_WinMainLoop)
 *   8. InstallHook() — IAT swap on user32!PeekMessageA
 *
 * After bootstrap, every PeekMessageA call from the game invokes WinMainLoop()
 * before forwarding to the original USER32 entry. Failure in any bootstrap
 * step keeps the game running unhooked — the log file is the only feedback
 * because chat is not available yet.
 * ========================================================================== */

/* ---------------- logging -------------------------------------------------- */

static HANDLE g_logFile = INVALID_HANDLE_VALUE;
static CRITICAL_SECTION g_logLock;
static BOOL g_logInit = FALSE;

static void LogOpen(const wchar_t *baseDir)
{
    wchar_t path[MAX_PATH];
    if (swprintf(path, MAX_PATH, L"%s\\sf_loader.log", baseDir) < 0)
    {
        return;
    }

    g_logFile = CreateFileW(path, FILE_APPEND_DATA, FILE_SHARE_READ, NULL,
                            OPEN_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL);
    if (g_logFile == INVALID_HANDLE_VALUE)
    {
        return;
    }

    InitializeCriticalSection(&g_logLock);
    g_logInit = TRUE;
}

static void LogWriteLine(const char *level, const char *line)
{
    if (!g_logInit)
    {
        return;
    }

    SYSTEMTIME st;
    GetLocalTime(&st);

    char buffer[1024];
    int n = _snprintf_s(buffer, sizeof(buffer), _TRUNCATE,
                        "[%04u-%02u-%02u %02u:%02u:%02u.%03u] [%s] %s\r\n",
                        st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute,
                        st.wSecond, st.wMilliseconds, level, line);
    if (n <= 0)
    {
        return;
    }

    EnterCriticalSection(&g_logLock);
    DWORD written = 0;
    WriteFile(g_logFile, buffer, (DWORD)n, &written, NULL);
    FlushFileBuffers(g_logFile);
    LeaveCriticalSection(&g_logLock);
}

static void LogInfo(const char *msg) { LogWriteLine("INFO", msg); }
static void LogError(const char *msg) { LogWriteLine("ERROR", msg); }

static void LogInfoW(const char *prefix, const wchar_t *value)
{
    char narrow[1024];
    char line[1280];
    int converted = WideCharToMultiByte(CP_UTF8, 0, value, -1, narrow, (int)sizeof(narrow), NULL, NULL);
    if (converted <= 0)
    {
        strcpy_s(narrow, sizeof(narrow), "<conversion failed>");
    }
    _snprintf_s(line, sizeof(line), _TRUNCATE, "%s %s", prefix, narrow);
    LogInfo(line);
}

static void LogErrorW(const char *prefix, const wchar_t *value)
{
    char narrow[1024];
    char line[1280];
    int converted = WideCharToMultiByte(CP_UTF8, 0, value, -1, narrow, (int)sizeof(narrow), NULL, NULL);
    if (converted <= 0)
    {
        strcpy_s(narrow, sizeof(narrow), "<conversion failed>");
    }
    _snprintf_s(line, sizeof(line), _TRUNCATE, "%s %s", prefix, narrow);
    LogError(line);
}

static void LogErrorHresult(const char *prefix, int rc)
{
    char line[256];
    _snprintf_s(line, sizeof(line), _TRUNCATE, "%s hr=0x%08X", prefix, (unsigned)rc);
    LogError(line);
}

/* ---------------- hostfxr bootstrap --------------------------------------- */

static HMODULE g_hostfxr = NULL;
static hostfxr_initialize_for_runtime_config_fn g_init_fptr = NULL;
static hostfxr_get_runtime_delegate_fn g_get_delegate_fptr = NULL;
static hostfxr_close_fn g_close_fptr = NULL;

typedef void(__stdcall *win_main_loop_fn)(void);
static win_main_loop_fn g_WinMainLoop = NULL;

static BOOL LoadHostfxr(void)
{
    wchar_t hostfxr_path[MAX_PATH];
    size_t buffer_size = MAX_PATH;
    int rc = get_hostfxr_path(hostfxr_path, &buffer_size, NULL);
    if (rc != 0)
    {
        LogErrorHresult("get_hostfxr_path failed", rc);
        return FALSE;
    }

    LogInfoW("get_hostfxr_path ok ->", hostfxr_path);

    g_hostfxr = LoadLibraryW(hostfxr_path);
    if (!g_hostfxr)
    {
        LogError("LoadLibraryW(hostfxr) failed");
        return FALSE;
    }

    g_init_fptr = (hostfxr_initialize_for_runtime_config_fn)GetProcAddress(g_hostfxr, "hostfxr_initialize_for_runtime_config");
    g_get_delegate_fptr = (hostfxr_get_runtime_delegate_fn)GetProcAddress(g_hostfxr, "hostfxr_get_runtime_delegate");
    g_close_fptr = (hostfxr_close_fn)GetProcAddress(g_hostfxr, "hostfxr_close");

    if (!g_init_fptr || !g_get_delegate_fptr || !g_close_fptr)
    {
        LogError("hostfxr exports not resolved");
        return FALSE;
    }

    LogInfo("hostfxr exports resolved");
    return TRUE;
}

static load_assembly_and_get_function_pointer_fn GetLoadAssemblyFn(const wchar_t *runtimeConfigPath)
{
    hostfxr_handle ctx = NULL;
    int rc = g_init_fptr(runtimeConfigPath, NULL, &ctx);
    if (rc != 0 || ctx == NULL)
    {
        LogErrorHresult("hostfxr_initialize_for_runtime_config failed", rc);
        if (ctx)
            g_close_fptr(ctx);
        return NULL;
    }

    LogInfo("hostfxr_initialize_for_runtime_config ok");

    load_assembly_and_get_function_pointer_fn load_fn = NULL;
    rc = g_get_delegate_fptr(ctx, hdt_load_assembly_and_get_function_pointer, (void **)&load_fn);
    g_close_fptr(ctx);

    if (rc != 0 || load_fn == NULL)
    {
        LogErrorHresult("hostfxr_get_runtime_delegate failed", rc);
        return NULL;
    }

    LogInfo("hdt_load_assembly_and_get_function_pointer acquired");
    return load_fn;
}

static BOOL BootstrapManagedEntry(const wchar_t *baseDir)
{
    if (!LoadHostfxr())
    {
        return FALSE;
    }

    wchar_t runtime_config[MAX_PATH];
    wchar_t assembly_path[MAX_PATH];
    if (swprintf(runtime_config, MAX_PATH, L"%s\\SF.Runtime.runtimeconfig.json", baseDir) < 0 ||
        swprintf(assembly_path, MAX_PATH, L"%s\\SF.Runtime.dll", baseDir) < 0)
    {
        LogError("path composition failed");
        return FALSE;
    }

    if (GetFileAttributesW(runtime_config) == INVALID_FILE_ATTRIBUTES)
    {
        LogErrorW("runtimeconfig not found at", runtime_config);
        return FALSE;
    }

    if (GetFileAttributesW(assembly_path) == INVALID_FILE_ATTRIBUTES)
    {
        LogErrorW("SF.Runtime.dll not found at", assembly_path);
        return FALSE;
    }

    load_assembly_and_get_function_pointer_fn load_fn = GetLoadAssemblyFn(runtime_config);
    if (!load_fn)
    {
        return FALSE;
    }

    int rc = load_fn(
        assembly_path,
        L"SFSharp.SFBootstrap, SF.Runtime",
        L"WinMainLoop",
        UNMANAGEDCALLERSONLY_METHOD,
        NULL,
        (void **)&g_WinMainLoop);

    if (rc != 0 || g_WinMainLoop == NULL)
    {
        LogErrorHresult("load_assembly_and_get_function_pointer(WinMainLoop) failed", rc);
        return FALSE;
    }

    LogInfo("managed WinMainLoop resolved");
    return TRUE;
}

/* ---------------- IAT hook on user32!PeekMessageA ------------------------- */

static BOOL(WINAPI *g_OrigPeekMessageA)(LPMSG, HWND, UINT, UINT, UINT) = NULL;
static FARPROC *g_pIATEntry = NULL;
static volatile LONG g_tickReentrancy = 0;

static BOOL WINAPI HookedPeekMessageA(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg)
{
    win_main_loop_fn tick = g_WinMainLoop;
    if (tick != NULL && InterlockedCompareExchange(&g_tickReentrancy, 1, 0) == 0)
    {
        __try
        {
            tick();
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
            LogError("SEH in managed WinMainLoop");
        }
        InterlockedExchange(&g_tickReentrancy, 0);
    }

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
    {
        LogError("GetModuleHandleA(NULL) failed");
        return FALSE;
    }

    g_pIATEntry = FindIATEntry(exe, "USER32.dll", "PeekMessageA");
    if (!g_pIATEntry)
    {
        LogError("PeekMessageA IAT entry not found");
        return FALSE;
    }

    g_OrigPeekMessageA = (BOOL(WINAPI *)(LPMSG, HWND, UINT, UINT, UINT)) * g_pIATEntry;

    DWORD oldProtect;
    if (!VirtualProtect(g_pIATEntry, sizeof(FARPROC), PAGE_READWRITE, &oldProtect))
    {
        LogError("VirtualProtect(RW) failed");
        return FALSE;
    }

    *g_pIATEntry = (FARPROC)HookedPeekMessageA;
    VirtualProtect(g_pIATEntry, sizeof(FARPROC), oldProtect, &oldProtect);
    LogInfo("IAT hook installed");
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

/* ---------------- base directory resolution ------------------------------- */

static BOOL ResolveBaseDirectory(HINSTANCE hinstDLL, wchar_t *outDir, size_t outCount)
{
    wchar_t path[MAX_PATH];
    DWORD length = GetModuleFileNameW(hinstDLL, path, MAX_PATH);
    if (length == 0 || length >= MAX_PATH)
    {
        return FALSE;
    }

    wchar_t *lastSlash = wcsrchr(path, L'\\');
    if (lastSlash == NULL)
    {
        return FALSE;
    }

    *lastSlash = L'\0';
    if (wcscpy_s(outDir, outCount, path) != 0)
    {
        return FALSE;
    }

    return TRUE;
}

/* ---------------- DllMain ------------------------------------------------- */

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpvReserved)
{
    if (fdwReason == DLL_PROCESS_ATTACH)
    {
        DisableThreadLibraryCalls(hinstDLL);

        wchar_t baseDir[MAX_PATH];
        if (!ResolveBaseDirectory(hinstDLL, baseDir, MAX_PATH))
        {
            return TRUE;
        }

        LogOpen(baseDir);
        LogInfo("===== SF.asi loader starting =====");
        LogInfoW("base dir:", baseDir);

        if (!BootstrapManagedEntry(baseDir))
        {
            LogError("managed bootstrap failed, game continues unhooked");
            return TRUE;
        }

        if (!InstallHook())
        {
            LogError("IAT hook install failed");
            return TRUE;
        }

        LogInfo("loader ready");
        return TRUE;
    }

    if (fdwReason == DLL_PROCESS_DETACH && lpvReserved == NULL)
    {
        RestoreHook();
    }

    return TRUE;
}
