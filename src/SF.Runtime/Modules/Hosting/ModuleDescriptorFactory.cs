using System.Reflection;

namespace SFSharp.Runtime.Modules.Hosting;

internal static class ModuleDescriptorFactory
{
    internal static ModuleDescriptor FromType(Type moduleType)
    {
        ArgumentNullException.ThrowIfNull(moduleType);
        SFModuleAttribute metadata = moduleType.GetCustomAttribute<SFModuleAttribute>()
            ?? throw new InvalidOperationException($"Module type {moduleType.FullName} is missing [SFModule].");
        if (string.IsNullOrWhiteSpace(metadata.Id))
            throw new InvalidOperationException($"Module type {moduleType.FullName} has an empty module id.");

        string id = metadata.Id.Trim();
        string[] dependencies = metadata.Dependencies
            .Where(static dependency => !string.IsNullOrWhiteSpace(dependency))
            .Select(static dependency => dependency.Trim())
            .Where(dependency => !string.Equals(dependency, id, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new(id,
            string.IsNullOrWhiteSpace(metadata.DisplayName) ? moduleType.Name : metadata.DisplayName.Trim(),
            string.IsNullOrWhiteSpace(metadata.Category) ? "General" : metadata.Category.Trim(),
            metadata.Description?.Trim() ?? string.Empty,
            metadata.DefaultEnabled, metadata.ExecutionModel, metadata.RestartPolicy, metadata.Order,
            Array.AsReadOnly(dependencies), moduleType.Name);
    }
}
