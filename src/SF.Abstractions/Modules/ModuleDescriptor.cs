namespace SFSharp.Abstractions.Modules;

/// <summary>Copied module metadata. Does not retain plugin types or assemblies.</summary>
public sealed record ModuleDescriptor(
    string Id,
    string DisplayName,
    string Category,
    string Description,
    bool DefaultEnabled,
    ModuleExecutionModel ExecutionModel,
    ModuleRestartPolicy RestartPolicy,
    int Order,
    IReadOnlyList<string> Dependencies,
    string ModuleTypeName);
