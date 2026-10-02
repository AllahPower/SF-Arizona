using System.Diagnostics.CodeAnalysis;

namespace SFSharp.Runtime.Modules.PluginLoading;

/// <summary>
/// The <see cref="ISFEarlyModule"/> instances of one plugin with their contexts, from plugin load to unload.
/// </summary>
internal sealed class EarlyModuleHost : IDisposable
{
    private readonly string _pluginId;
    private readonly List<(ISFEarlyModule Module, EarlyContext Context)> _modules = [];

    private EarlyModuleHost(string pluginId)
    {
        _pluginId = pluginId;
    }

    public int Count => _modules.Count;

    public static Type[] FindTypes(IEnumerable<Type> types, System.Reflection.Assembly assembly)
    {
        return types
            .Where(type => !type.IsAbstract && !type.IsInterface && type.Assembly == assembly && typeof(ISFEarlyModule).IsAssignableFrom(type))
            .ToArray();
    }

    /// <summary>
    /// Creates every early module and calls <see cref="ISFEarlyModule.OnGameLoading"/>. On failure the modules
    /// started so far are disposed and the exception propagates.
    /// </summary>
    [RequiresDynamicCode("Activator.CreateInstance relies on reflection and is unavailable under NativeAOT.")]
    public static EarlyModuleHost Start(string pluginId, IReadOnlyList<Type> types, ISFGameLoading loading)
    {
        EarlyModuleHost host = new(pluginId);
        try
        {
            foreach (Type type in types)
            {
                if (type.GetConstructor(Type.EmptyTypes) is null)
                {
                    throw new InvalidOperationException($"Early module {type.FullName} has no public parameterless constructor.");
                }

                ISFEarlyModule module = (ISFEarlyModule)Activator.CreateInstance(type)!;
                EarlyContext context = new(pluginId, type, loading);
                host._modules.Add((module, context));
                module.OnGameLoading(context);
                SFLog.Debug($"PluginLoader[{pluginId}]: early module {type.FullName} started at stage {loading.Stage}");
            }
        }
        catch
        {
            host.Dispose();
            throw;
        }

        return host;
    }

    public void Dispose()
    {
        for (int i = _modules.Count - 1; i >= 0; i--)
        {
            (ISFEarlyModule module, EarlyContext context) = _modules[i];
            context.Dispose();
            try
            {
                (module as IDisposable)?.Dispose();
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, $"PluginLoader[{_pluginId}]: early module {module.GetType().FullName} Dispose failed");
            }
        }

        _modules.Clear();
    }
}
