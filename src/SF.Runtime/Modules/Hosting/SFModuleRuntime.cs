namespace SFSharp.Runtime.Modules.Hosting;

/// <summary>
/// Host-side module runtime settings, such as the pluggable <see cref="IModuleStorageProvider"/>.
/// Owned by <see cref="SFHost"/>. Not to be confused with <see cref="ISFModules"/>, which
/// is the read-only public catalog of registered modules.
/// </summary>
/// <remarks>
/// <see cref="Storage"/> getter/setter is thread-safe but should be configured once during host
/// startup. Concurrent mutation after modules have started is not supported.
/// </remarks>
public sealed class SFModuleRuntime
{
    private IModuleStorageProvider _storage = new DefaultModuleStorageProvider();

    public IModuleStorageProvider Storage
    {
        get => _storage;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _storage = value;
        }
    }

    public DefaultModuleStorageProvider? DefaultStorage => _storage as DefaultModuleStorageProvider;
}
