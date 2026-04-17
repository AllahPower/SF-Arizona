using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace SFSharp;

internal static class ModuleExecutionStateStore
{
    private sealed class ModuleExecutionState
    {
        public IModuleContext? Context { get; set; }
        public ILogger? Log { get; set; }
    }

    private static readonly ConditionalWeakTable<ISFModule, ModuleExecutionState> States = new();

    internal static void Attach(ISFModule module, IModuleContext context, ILogger log)
    {
        ModuleExecutionState state = States.GetValue(module, static _ => new());
        state.Context = context;
        state.Log = log;
    }

    internal static void Clear(ISFModule module)
    {
        if (States.TryGetValue(module, out ModuleExecutionState? state))
        {
            state.Context = null;
            state.Log = null;
        }
    }

    internal static IModuleContext? GetContext(ISFModule module)
    {
        return States.TryGetValue(module, out ModuleExecutionState? state)
            ? state.Context
            : null;
    }

    internal static ILogger? GetLog(ISFModule module)
    {
        return States.TryGetValue(module, out ModuleExecutionState? state)
            ? state.Log
            : null;
    }
}
