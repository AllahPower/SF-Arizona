#include <windows.h>

/* ============================================================================
 * EXPERIMENT: JIT runtime host (branch experiment/jit-runtime)
 * ----------------------------------------------------------------------------
 * Цель: уйти от NativeAOT и сделать SF.asi полноценным загрузчиком .NET
 * рантайма, чтобы:
 *   1. встроенные модули жили как сейчас
 *   2. сторонние плагины (.dll) подгружались динамически из <GameDir>\SF\modules\*
 *   3. был доступен hot-reload через collectible AssemblyLoadContext
 *
 * Текущая модель (AOT):
 *   dllmain.c статически слинкован с WinMainLoop() из SFBootstrap.cs,
 *   потому что NativeAOT экспортирует [UnmanagedCallersOnly] метод как
 *   обычный PE-символ и линкер всё сшивает в один SF.asi.
 *
 * Целевая модель (JIT):
 *   dllmain.c больше не имеет прямой ссылки на C#. Управляемый код живёт
 *   в SF.Runtime.dll и загружается CoreCLR-ом в рантайме. dllmain через
 *   nethost/hostfxr поднимает CoreCLR и просит у него function pointer
 *   на SFBootstrap.WinMainLoop, который дальше дёргает IAT-хук.
 *
 * Поэтапный план внедрения:
 *
 *   Этап 1. Выделить публичный контракт
 *     - Создать csproj SF.Abstractions (TargetFramework=net10.0, без AOT)
 *     - Перенести: ISFModule, SFModuleAttribute, SFModuleBase, ModuleContext,
 *       ModuleDescriptor, IModuleStorage, IModuleConfig, фасад SF, SFEvents,
 *       SFChat, SFColors и прочие public-типы из src/SF/.
 *     - Хост и плагины ссылаются ТОЛЬКО на SF.Abstractions.
 *
 *   Этап 2. Переключить хост с AOT на CoreCLR
 *     - Снять <PublishAot>true</PublishAot> из SF.csproj.
 *     - Удалить <SelfContained>true</SelfContained>, оставить frame-dependent
 *       или собрать с публикацией CoreCLR в подпапку SF\runtime\.
 *     - Собирать SF.asi из этого dllmain.c как отдельный native проект
 *       (cl.exe /LD или cmake), линковать с nethost.lib.
 *     - Положить рядом SF.Runtime.dll + SF.Runtime.runtimeconfig.json
 *       с указанием framework Microsoft.NETCore.App 10.0.
 *
 *   Этап 3. Hostfxr-bootstrap в этом файле
 *     В DllMain → DLL_PROCESS_ATTACH:
 *       a) get_hostfxr_path()       (из nethost.dll)
 *       b) LoadLibrary(hostfxr)     получить hostfxr_initialize_for_runtime_config,
 *                                   hostfxr_get_runtime_delegate, hostfxr_close
 *       c) init(L"SF.Runtime.runtimeconfig.json", NULL, &ctx)
 *       d) get_runtime_delegate(ctx, hdt_load_assembly_and_get_function_pointer,
 *                               &loader_fn)
 *       e) loader_fn(L"SF.Runtime.dll",
 *                    L"SFSharp.SFBootstrap, SF.Runtime",
 *                    L"WinMainLoop",
 *                    UNMANAGEDCALLERSONLY_METHOD,
 *                    NULL,
 *                    &g_WinMainLoop)
 *       f) InstallHook() — IAT-хук на PeekMessageA как сейчас
 *
 *   Этап 4. Изменить вызов в HookedPeekMessageA
 *     Вместо statically linked WinMainLoop() — вызов через указатель:
 *       typedef void (__stdcall *win_main_loop_fn)(void);
 *       static win_main_loop_fn g_WinMainLoop = NULL;
 *       if (g_WinMainLoop) g_WinMainLoop();
 *     Сам IAT-хук остаётся без изменений (это чистый Win32, .NET тут ни при чём).
 *
 *   Этап 5. PluginLoader на стороне C#
 *     - PluginLoadContext : AssemblyLoadContext (collectible, isolated)
 *     - AssemblyDependencyResolver для plugin.deps.json
 *     - SharedAssemblies: SF.Abstractions, Microsoft.Extensions.Logging.Abstractions,
 *       System.Text.Json — возвращать null из Load(), чтобы шли через Default ALC,
 *       иначе typeof(ISFModule) из плагина != typeof(ISFModule) из хоста.
 *     - Сканер <GameDir>\SF\modules\* /module.json
 *     - Регистрация типов с [SFModule] через перегрузку RegisterModule(Type)
 *     - Версионная проверка manifest.minHostVersion vs SF.Runtime version
 *
 *   Этап 6. Hot-reload
 *     /sfs reload <id> → SFModuleContainer.StopModule + PluginLoader.Unload(id)
 *     PluginLoadContext.Unload() + цикл GC.Collect/WaitForPendingFinalizers
 *     до WeakReference.IsAlive == false.
 *
 * MinHook под JIT — работает без изменений:
 *   - MinHook.NET это P/Invoke над нативным MinHook
 *   - Callbacks как [UnmanagedCallersOnly] static методы дают стабильный
 *     reverse-pinvoke stub (адрес стабилен даже при tiered re-jit)
 *   - Marshal.GetFunctionPointerForDelegate тоже доступен (под AOT был запрещён)
 *   - Trampoline на оригинал зовётся через delegate*<...> или GetDelegateForFunctionPointer
 *
 * Совместимость:
 *   Все существующие хуки (RakNet RPC/Packet, чат, диалог, инпут, скорборд)
 *   переезжают без изменений кода. Меняется только сборка хоста и стартап.
 * ============================================================================ */

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
