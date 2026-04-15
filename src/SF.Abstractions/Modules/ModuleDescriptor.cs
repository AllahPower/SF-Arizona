using System.Reflection;

namespace SFSharp;

/// <summary>
/// Immutable snapshot of the metadata declared on a module type. Built once at registration time
/// from <see cref="SFModuleAttribute"/> and reused across restarts. Surfaced through
/// <see cref="ModuleContext.Descriptor"/> and <see cref="ModuleRuntimeSnapshot.Descriptor"/>.
/// </summary>
/// <param name="Id">Stable module identifier, see <see cref="SFModuleAttribute.Id"/>.</param>
/// <param name="DisplayName">Human readable name, see <see cref="SFModuleAttribute.DisplayName"/>.</param>
/// <param name="Category">Grouping label, see <see cref="SFModuleAttribute.Category"/>.</param>
/// <param name="Description">Short description, see <see cref="SFModuleAttribute.Description"/>.</param>
/// <param name="DefaultEnabled">Autostart default, see <see cref="SFModuleAttribute.DefaultEnabled"/>.</param>
/// <param name="ExecutionModel">Threading model, see <see cref="SFModuleAttribute.ExecutionModel"/>.</param>
/// <param name="RestartPolicy">Fault handling policy, see <see cref="SFModuleAttribute.RestartPolicy"/>.</param>
/// <param name="Order">Soft start order, see <see cref="SFModuleAttribute.Order"/>.</param>
/// <param name="Dependencies">Declared module id dependencies, see <see cref="SFModuleAttribute.Dependencies"/>.</param>
/// <param name="ModuleType">CLR type of the module, used by the container's factory.</param>
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
    Type ModuleType)
{
    /// <summary>
    /// Reads <see cref="SFModuleAttribute"/> from <paramref name="moduleType"/> and produces the
    /// descriptor. Used by <see cref="SFModuleContainer.RegisterModule{T}(bool?)"/>.
    /// </summary>
    /// <param name="moduleType">Concrete module type decorated with <see cref="SFModuleAttribute"/>.</param>
    /// <exception cref="ArgumentNullException"><paramref name="moduleType"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The type is missing the attribute or declares an empty id.</exception>
    public static ModuleDescriptor FromType(Type moduleType)
    {
        ArgumentNullException.ThrowIfNull(moduleType);
        SFModuleAttribute? metadata = moduleType.GetCustomAttribute<SFModuleAttribute>();
        if (metadata is null)
        {
            throw new InvalidOperationException($"Module type {moduleType.FullName} is missing [SFModule].");
        }

        if (string.IsNullOrWhiteSpace(metadata.Id))
        {
            throw new InvalidOperationException($"Module type {moduleType.FullName} has an empty module id.");
        }

        string moduleId = metadata.Id.Trim();
        string[] dependencies = metadata.Dependencies
            .Where(static dependency => !string.IsNullOrWhiteSpace(dependency))
            .Select(static dependency => dependency.Trim())
            .Where(dependency => !string.Equals(dependency, moduleId, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new(
            moduleId,
            string.IsNullOrWhiteSpace(metadata.DisplayName) ? moduleType.Name : metadata.DisplayName.Trim(),
            string.IsNullOrWhiteSpace(metadata.Category) ? "General" : metadata.Category.Trim(),
            metadata.Description?.Trim() ?? string.Empty,
            metadata.DefaultEnabled,
            metadata.ExecutionModel,
            metadata.RestartPolicy,
            metadata.Order,
            Array.AsReadOnly(dependencies),
            moduleType);
    }
}
