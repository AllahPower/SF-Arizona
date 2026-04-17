namespace SFSharp;

/// <summary>Read-only catalog of modules currently known to the host.</summary>
/// <remarks>Thread-safe - all queries read an immutable snapshot.</remarks>
public interface ISFModules
{
    /// <summary>Returns a point-in-time snapshot of every registered module.</summary>
    IReadOnlyCollection<SFModuleInfo> GetAll();

    /// <summary>Gets a module snapshot by id, or <see langword="null"/> when not found.</summary>
    SFModuleInfo? Get(string moduleId);

    /// <summary>Attempts to get a module snapshot by id.</summary>
    bool TryGet(string moduleId, out SFModuleInfo module);
}
