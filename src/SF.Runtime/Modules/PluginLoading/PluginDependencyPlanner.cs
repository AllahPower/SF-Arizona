namespace SFSharp.Runtime.Modules.PluginLoading;

internal sealed record PlannedPlugin(ResolvedPluginManifest Manifest, IReadOnlyList<string> Warnings);

internal sealed record RejectedPlugin(ResolvedPluginManifest Manifest, PluginLoadFailureReason Reason, string Message);

internal sealed record PluginLoadPlan(IReadOnlyList<PlannedPlugin> Ordered, IReadOnlyList<RejectedPlugin> Rejected);

/// <summary>
/// Decides which plugin candidates may load and in which order, from their declared dependencies,
/// the host version and the plugins that are already loaded. Pure: performs no I/O or loading.
/// </summary>
internal static class PluginDependencyPlanner
{
    /// <param name="hostVersion">Compared as its numeric core, because preview hosts carry a prerelease tag
    /// (3.3.1-preview.N) that would otherwise rank below plugins built from the same 3.3.1 sources.</param>
    public static PluginLoadPlan Plan(
        IReadOnlyList<ResolvedPluginManifest> candidates,
        SemanticVersion hostVersion,
        IReadOnlyDictionary<string, SemanticVersion> loadedPlugins)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(hostVersion);
        ArgumentNullException.ThrowIfNull(loadedPlugins);

        SemanticVersion host = hostVersion.CoreVersion;
        Dictionary<string, RejectedPlugin> rejected = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, ResolvedPluginManifest> byId = new(StringComparer.OrdinalIgnoreCase);
        List<RejectedPlugin> duplicates = [];

        foreach (ResolvedPluginManifest candidate in candidates)
        {
            if (loadedPlugins.ContainsKey(candidate.PluginId) || !byId.TryAdd(candidate.PluginId, candidate))
            {
                duplicates.Add(new RejectedPlugin(candidate, PluginLoadFailureReason.AlreadyLoaded,
                    $"Plugin '{candidate.PluginId}' is already loaded or declared by another manifest."));
            }
        }

        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (ResolvedPluginManifest candidate in byId.Values)
            {
                if (!rejected.ContainsKey(candidate.PluginId)
                    && FindDependencyProblem(candidate, host, byId, rejected, loadedPlugins) is RejectedPlugin problem)
                {
                    rejected.Add(candidate.PluginId, problem);
                    changed = true;
                }
            }
        }

        List<ResolvedPluginManifest> pending = [.. byId.Values.Where(candidate => !rejected.ContainsKey(candidate.PluginId))];
        List<PlannedPlugin> ordered = [];
        HashSet<string> placed = new(StringComparer.OrdinalIgnoreCase);
        while (pending.Count != 0)
        {
            // Earliest ready candidate first keeps discovery order for plugins without dependencies.
            ResolvedPluginManifest? ready = pending.FirstOrDefault(candidate => candidate.Dependencies
                .Where(static dependency => !dependency.IsHost)
                .All(dependency => loadedPlugins.ContainsKey(dependency.Id) || placed.Contains(dependency.Id)));
            if (ready is null)
            {
                string cycle = string.Join(", ", pending.Select(static candidate => candidate.PluginId));
                foreach (ResolvedPluginManifest member in pending)
                {
                    rejected.Add(member.PluginId, new RejectedPlugin(member, PluginLoadFailureReason.DependencyCycle,
                        $"Plugin '{member.PluginId}' is part of a dependency cycle: {cycle}."));
                }

                break;
            }

            pending.Remove(ready);
            placed.Add(ready.PluginId);
            ordered.Add(new PlannedPlugin(ready, CollectWarnings(ready, host, byId, loadedPlugins)));
        }

        return new PluginLoadPlan(ordered, [.. duplicates, .. rejected.Values]);
    }

    private static RejectedPlugin? FindDependencyProblem(
        ResolvedPluginManifest candidate,
        SemanticVersion host,
        Dictionary<string, ResolvedPluginManifest> candidates,
        Dictionary<string, RejectedPlugin> rejected,
        IReadOnlyDictionary<string, SemanticVersion> loadedPlugins)
    {
        foreach (ResolvedPluginDependency dependency in candidate.Dependencies)
        {
            SemanticVersion? actual;
            if (dependency.IsHost)
            {
                actual = host;
            }
            else if (rejected.ContainsKey(dependency.Id))
            {
                return new RejectedPlugin(candidate, PluginLoadFailureReason.DependencyMissing,
                    $"Plugin '{candidate.PluginId}' requires '{dependency.Id}', which could not be loaded.");
            }
            else if (!TryGetVersion(dependency.Id, candidates, loadedPlugins, out actual))
            {
                return new RejectedPlugin(candidate, PluginLoadFailureReason.DependencyMissing,
                    $"Plugin '{candidate.PluginId}' requires '{dependency.Id}' ({dependency.Range}), which is not installed.");
            }

            if (!dependency.Range.Contains(actual!))
            {
                return new RejectedPlugin(candidate, PluginLoadFailureReason.DependencyVersionOutOfRange,
                    $"Plugin '{candidate.PluginId}' supports '{dependency.Id}' {dependency.Range}, but {actual} is installed.");
            }
        }

        return null;
    }

    private static List<string> CollectWarnings(
        ResolvedPluginManifest candidate,
        SemanticVersion host,
        Dictionary<string, ResolvedPluginManifest> candidates,
        IReadOnlyDictionary<string, SemanticVersion> loadedPlugins)
    {
        List<string> warnings = [.. candidate.Warnings];
        foreach (ResolvedPluginDependency dependency in candidate.Dependencies)
        {
            if (dependency.Range.Target is not SemanticVersion target)
            {
                continue;
            }

            SemanticVersion? actual = dependency.IsHost ? host : null;
            if (actual is null && !TryGetVersion(dependency.Id, candidates, loadedPlugins, out actual))
            {
                continue;
            }

            bool matches = dependency.IsHost ? actual! == target.CoreVersion : actual! == target;
            if (!matches)
            {
                warnings.Add($"not tested with '{dependency.Id}' {actual} (target {target})");
            }
        }

        return warnings;
    }

    private static bool TryGetVersion(
        string pluginId,
        Dictionary<string, ResolvedPluginManifest> candidates,
        IReadOnlyDictionary<string, SemanticVersion> loadedPlugins,
        out SemanticVersion? version)
    {
        if (loadedPlugins.TryGetValue(pluginId, out version))
        {
            return true;
        }

        version = candidates.TryGetValue(pluginId, out ResolvedPluginManifest? candidate) ? candidate.Version : null;
        return version is not null;
    }
}
