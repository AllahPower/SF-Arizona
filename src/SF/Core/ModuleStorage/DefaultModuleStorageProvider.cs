using System.Collections.Concurrent;

namespace SFSharp.Runtime.Modules;

public sealed class DefaultModuleStorageProvider : IModuleStorageProvider
{
    private readonly ConcurrentDictionary<string, IModuleStorage> _assetsOverrides = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IModuleStorage> _userDataOverrides = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IModuleConfig> _configOverrides = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IModuleStorage> _assetsCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IModuleStorage> _userDataCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IModuleConfig> _configCache = new(StringComparer.OrdinalIgnoreCase);

    public bool AssetsReadOnly { get; init; }

    public IModuleStorage GetAssets(ModuleDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (_assetsOverrides.TryGetValue(descriptor.Id, out IModuleStorage? overridden))
        {
            return overridden;
        }

        return _assetsCache.GetOrAdd(descriptor.Id, id =>
            new FileSystemModuleStorage(SFPaths.GetModuleAssetsDirectory(id), AssetsReadOnly));
    }

    public IModuleStorage GetUserData(ModuleDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (_userDataOverrides.TryGetValue(descriptor.Id, out IModuleStorage? overridden))
        {
            return overridden;
        }

        return _userDataCache.GetOrAdd(descriptor.Id, id =>
            new FileSystemModuleStorage(SFPaths.GetModuleUserDataDirectory(id)));
    }

    public IModuleConfig GetConfig(ModuleDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        if (_configOverrides.TryGetValue(descriptor.Id, out IModuleConfig? overridden))
        {
            return overridden;
        }

        return _configCache.GetOrAdd(descriptor.Id, _ =>
            new JsonModuleConfig(GetUserData(descriptor)));
    }

    public void OverrideAssets(string moduleId, IModuleStorage storage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);
        ArgumentNullException.ThrowIfNull(storage);
        _assetsOverrides[moduleId] = storage;
    }

    public void OverrideUserData(string moduleId, IModuleStorage storage)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);
        ArgumentNullException.ThrowIfNull(storage);
        _userDataOverrides[moduleId] = storage;
    }

    public void OverrideConfig(string moduleId, IModuleConfig config)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);
        ArgumentNullException.ThrowIfNull(config);
        _configOverrides[moduleId] = config;
    }

    public bool ClearAssetsOverride(string moduleId) => _assetsOverrides.TryRemove(moduleId, out _);
    public bool ClearUserDataOverride(string moduleId) => _userDataOverrides.TryRemove(moduleId, out _);
    public bool ClearConfigOverride(string moduleId) => _configOverrides.TryRemove(moduleId, out _);
}
