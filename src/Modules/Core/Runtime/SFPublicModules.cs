using System.Collections.ObjectModel;

namespace SFSharp;

internal sealed class SFPublicModules : ISFModules
{
    private static readonly IReadOnlyCollection<SFModuleInfo> Empty = Array.Empty<SFModuleInfo>();

    public static SFPublicModules Instance { get; } = new();

    private IReadOnlyCollection<SFModuleInfo> _snapshot = Empty;

    private SFPublicModules()
    {
    }

    public IReadOnlyCollection<SFModuleInfo> GetAll()
    {
        return Volatile.Read(ref _snapshot);
    }

    public SFModuleInfo? Get(string moduleId)
    {
        return TryGet(moduleId, out SFModuleInfo module) ? module : null;
    }

    public bool TryGet(string moduleId, out SFModuleInfo module)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(moduleId);
        foreach (SFModuleInfo entry in GetAll())
        {
            if (string.Equals(entry.Id, moduleId, StringComparison.OrdinalIgnoreCase))
            {
                module = entry;
                return true;
            }
        }

        module = null!;
        return false;
    }

    internal void Publish(IEnumerable<SFModuleInfo> modules)
    {
        ArgumentNullException.ThrowIfNull(modules);
        IReadOnlyCollection<SFModuleInfo> snapshot = new ReadOnlyCollection<SFModuleInfo>(modules.ToArray());
        Volatile.Write(ref _snapshot, snapshot);
    }
}
