using SFSharp.Runtime.Modules.PluginLoading;

namespace SF.Network.Tests;

public sealed class SemanticVersionTests
{
    [Theory]
    [InlineData("1.0.0-alpha", "1.0.0-alpha.1")]
    [InlineData("1.0.0-alpha.1", "1.0.0-alpha.beta")]
    [InlineData("1.0.0-alpha.beta", "1.0.0-beta")]
    [InlineData("1.0.0-beta.2", "1.0.0-beta.11")]
    [InlineData("1.0.0-rc.1", "1.0.0")]
    [InlineData("1.9.0", "1.10.0")]
    public void OrdersVersionsAsSemVerSpecifies(string lower, string higher)
    {
        Assert.True(SemanticVersion.Parse(lower) < SemanticVersion.Parse(higher));
    }

    [Fact]
    public void IgnoresBuildMetadataInComparisons()
    {
        Assert.Equal(SemanticVersion.Parse("3.3.1+abc"), SemanticVersion.Parse("3.3.1+def"));
    }

    [Theory]
    [InlineData("3.3")]
    [InlineData("03.3.1")]
    [InlineData("3.3.1-01")]
    [InlineData("3.3.1-")]
    [InlineData("v3.3.1")]
    public void RejectsInvalidVersions(string text)
    {
        Assert.False(SemanticVersion.TryParse(text, out _));
    }
}

public sealed class PluginDependencyRangeTests
{
    [Theory]
    [InlineData("3.x", "3.9.4", true)]
    [InlineData("3.x", "3.9.0-beta", true)]
    [InlineData("3.x", "4.0.0-alpha", false)]
    [InlineData("3.4.x", "3.4.9", true)]
    [InlineData("3.4.x", "3.5.0", false)]
    [InlineData("3.4.2", "3.4.2", true)]
    [InlineData("3.4.2", "3.4.3-alpha", false)]
    public void TreatsMaxAsInclusiveWithWildcards(string max, string version, bool expected)
    {
        Assert.True(PluginDependencyRange.TryParse("3.0.0", max, null, out PluginDependencyRange? range, out _));
        Assert.Equal(expected, range!.Contains(SemanticVersion.Parse(version)));
    }

    [Fact]
    public void MinIsInclusiveAndExcludesItsPrereleases()
    {
        Assert.True(PluginDependencyRange.TryParse("3.3.1", null, null, out PluginDependencyRange? range, out _));
        Assert.True(range!.Contains(SemanticVersion.Parse("3.3.1")));
        Assert.False(range.Contains(SemanticVersion.Parse("3.3.1-rc.1")));
    }

    [Theory]
    [InlineData("3.5.0", "3.4.x", null)]
    [InlineData("3.3.1", "3.x", "3.3.0")]
    [InlineData(null, "3.4.x", "3.5.0")]
    public void RejectsContradictoryBounds(string? min, string max, string? target)
    {
        Assert.False(PluginDependencyRange.TryParse(min, max, target, out _, out string? error));
        Assert.NotNull(error);
    }

    [Fact]
    public void FillsMissingBoundsFromCompiledVersion()
    {
        PluginDependencyRange range = PluginDependencyRange.Empty.WithDefaults(SemanticVersion.Parse("3.3.1"));

        Assert.Equal(SemanticVersion.Parse("3.3.1"), range.Min);
        Assert.Equal(SemanticVersion.Parse("3.3.1"), range.Target);
        Assert.Equal("3.x", range.Max);
        Assert.False(range.Contains(SemanticVersion.Parse("4.0.0")));
    }
}

public sealed class PluginDependencyPlannerTests
{
    private static readonly SemanticVersion Host = SemanticVersion.Parse("3.3.1");
    private static readonly Dictionary<string, SemanticVersion> NoneLoaded = new(StringComparer.OrdinalIgnoreCase);

    [Fact]
    public void LoadsDependenciesBeforeDependentsAndKeepsDiscoveryOrderOtherwise()
    {
        ResolvedPluginManifest demo = Manifest("demo", "1.0.0", ("ui", "0.3.0", "0.3.x", null));
        ResolvedPluginManifest chat = Manifest("chat", "1.0.0");
        ResolvedPluginManifest ui = Manifest("ui", "0.3.1");

        PluginLoadPlan plan = PluginDependencyPlanner.Plan([demo, chat, ui], Host, NoneLoaded);

        Assert.Empty(plan.Rejected);
        Assert.Equal(["chat", "ui", "demo"], plan.Ordered.Select(static planned => planned.Manifest.PluginId));
    }

    [Fact]
    public void RejectsMissingDependencyAndCascadesToItsDependents()
    {
        ResolvedPluginManifest middle = Manifest("middle", "1.0.0", ("absent", null, null, null));
        ResolvedPluginManifest top = Manifest("top", "1.0.0", ("middle", null, null, null));

        PluginLoadPlan plan = PluginDependencyPlanner.Plan([top, middle], Host, NoneLoaded);

        Assert.Empty(plan.Ordered);
        Assert.All(plan.Rejected, static rejected => Assert.Equal(PluginLoadFailureReason.DependencyMissing, rejected.Reason));
        Assert.Equal(2, plan.Rejected.Count);
    }

    [Fact]
    public void RejectsHostOutsideSupportedRange()
    {
        ResolvedPluginManifest plugin = Manifest("old", "1.0.0", ("sf", "3.0.0", "3.2.x", null));

        PluginLoadPlan plan = PluginDependencyPlanner.Plan([plugin], Host, NoneLoaded);

        RejectedPlugin rejected = Assert.Single(plan.Rejected);
        Assert.Equal(PluginLoadFailureReason.DependencyVersionOutOfRange, rejected.Reason);
    }

    [Fact]
    public void ComparesPreviewHostByItsCoreVersion()
    {
        ResolvedPluginManifest plugin = Manifest("chat", "1.0.0", ("sf", "3.3.1", "3.x", "3.3.1"));

        PluginLoadPlan plan = PluginDependencyPlanner.Plan([plugin], SemanticVersion.Parse("3.3.1-preview.43.1234567"), NoneLoaded);

        PlannedPlugin planned = Assert.Single(plan.Ordered);
        Assert.Empty(planned.Warnings);
    }

    [Fact]
    public void WarnsWhenRunningOnAVersionOtherThanTarget()
    {
        ResolvedPluginManifest plugin = Manifest("chat", "1.0.0", ("sf", "3.3.0", "3.x", "3.3.0"));

        PluginLoadPlan plan = PluginDependencyPlanner.Plan([plugin], Host, NoneLoaded);

        string warning = Assert.Single(Assert.Single(plan.Ordered).Warnings);
        Assert.Contains("target 3.3.0", warning);
    }

    [Fact]
    public void RejectsDependencyCycles()
    {
        ResolvedPluginManifest a = Manifest("a", "1.0.0", ("b", null, null, null));
        ResolvedPluginManifest b = Manifest("b", "1.0.0", ("a", null, null, null));

        PluginLoadPlan plan = PluginDependencyPlanner.Plan([a, b], Host, NoneLoaded);

        Assert.Empty(plan.Ordered);
        Assert.All(plan.Rejected, static rejected => Assert.Equal(PluginLoadFailureReason.DependencyCycle, rejected.Reason));
    }

    [Fact]
    public void AcceptsDependencyThatIsAlreadyLoaded()
    {
        ResolvedPluginManifest demo = Manifest("demo", "1.0.0", ("ui", "0.3.0-alpha.2", "0.3.x", null));
        Dictionary<string, SemanticVersion> loaded = new(StringComparer.OrdinalIgnoreCase) { ["ui"] = SemanticVersion.Parse("0.3.0-alpha.2") };

        PluginLoadPlan plan = PluginDependencyPlanner.Plan([demo], Host, loaded);

        Assert.Single(plan.Ordered);
    }

    private static ResolvedPluginManifest Manifest(string id, string version, params (string Id, string? Min, string? Max, string? Target)[] dependencies)
    {
        List<ResolvedPluginDependency> resolved = [];
        foreach ((string dependencyId, string? min, string? max, string? target) in dependencies)
        {
            Assert.True(PluginDependencyRange.TryParse(min, max, target, out PluginDependencyRange? range, out string? error), error);
            resolved.Add(new ResolvedPluginDependency(dependencyId, range!, IsInferred: false));
        }

        return new ResolvedPluginManifest(
            id, $"{id}/manifest.json", id, $"{id}/{id}.dll", null,
            SemanticVersion.Parse(version), resolved, [],
            new PluginManifestMetadata(id, version, null, null, null), new PluginManifest());
    }
}

public sealed class PluginManifestResolverTests : IDisposable
{
    private readonly string _root = Directory.CreateTempSubdirectory("sf-manifest-").FullName;

    [Fact]
    public void InfersHostRangeFromReferencedAbstractions()
    {
        string manifest = WritePlugin(typeof(PluginLoader).Assembly.Location, """{ "id": "p", "version": "1.0.0", "assembly": "p.dll" }""");

        ResolvedPluginDependency host = Assert.Single(Resolve(manifest).Dependencies);

        SemanticVersion abstractions = SemanticVersion.FromAssemblyVersion(typeof(SFSharp.Abstractions.ISF).Assembly.GetName().Version!);
        Assert.True(host.IsHost);
        Assert.True(host.IsInferred);
        Assert.Equal(abstractions, host.Range.Min);
        Assert.Equal(abstractions, host.Range.Target);
        Assert.Equal($"{abstractions.Major}.x", host.Range.Max);
    }

    [Fact]
    public void KeepsExplicitHostBoundsAndFillsOnlyMissingOnes()
    {
        string manifest = WritePlugin(typeof(PluginLoader).Assembly.Location,
            """{ "id": "p", "version": "1.0.0", "assembly": "p.dll", "dependencies": { "sf": { "max": "99.x" } } }""");

        ResolvedPluginDependency host = Assert.Single(Resolve(manifest).Dependencies);

        Assert.Equal("99.x", host.Range.Max);
        Assert.False(host.IsInferred);
        Assert.NotNull(host.Range.Min);
    }

    [Fact]
    public void WarnsWhenHostVersionCannotBeInferred()
    {
        string manifest = WritePlugin(typeof(SFSharp.Abstractions.ISF).Assembly.Location, """{ "id": "p", "version": "1.0.0", "assembly": "p.dll" }""");

        ResolvedPluginManifest resolved = Resolve(manifest);

        Assert.Empty(resolved.Dependencies);
        Assert.Single(resolved.Warnings);
    }

    [Theory]
    [InlineData("""{ "id": "p", "assembly": "p.dll" }""", PluginManifestResolutionFailureReason.MissingVersion)]
    [InlineData("""{ "id": "p", "version": "1.0", "assembly": "p.dll" }""", PluginManifestResolutionFailureReason.InvalidVersion)]
    [InlineData("""{ "id": "sf", "version": "1.0.0", "assembly": "p.dll" }""", PluginManifestResolutionFailureReason.ReservedPluginId)]
    [InlineData("""{ "id": "p", "version": "1.0.0", "assembly": "p.dll", "dependencies": { "p": {} } }""", PluginManifestResolutionFailureReason.InvalidDependency)]
    [InlineData("""{ "id": "p", "version": "1.0.0", "assembly": "p.dll", "dependencies": { "ui": { "max": "next" } } }""", PluginManifestResolutionFailureReason.InvalidDependency)]
    [InlineData("""{ "id": "p", "version": "1.0.0", "assembly": "p.dll", "sharedAssemblies": ["../lib"] }""", PluginManifestResolutionFailureReason.InvalidSharedAssembly)]
    [InlineData("""{ "id": "p", "version": "1.0.0", "assembly": "p.dll", "sharedAssemblies": ["SF.Abstractions"] }""", PluginManifestResolutionFailureReason.InvalidSharedAssembly)]
    [InlineData("""{ "id": "p", "version": "1.0.0", "assembly": "p.dll", "sharedAssemblies": ["p"] }""", PluginManifestResolutionFailureReason.InvalidSharedAssembly)]
    [InlineData("""{ "id": "p", "version": "1.0.0", "assembly": "p.dll", "sharedAssemblies": ["Missing.Lib"] }""", PluginManifestResolutionFailureReason.InvalidSharedAssembly)]
    [InlineData("""{ "id": "p", "version": "1.0.0", "assembly": "p.dll", "sharedAssemblies": ["lib", "LIB"] }""", PluginManifestResolutionFailureReason.InvalidSharedAssembly)]
    public void RejectsInvalidManifests(string json, PluginManifestResolutionFailureReason expected)
    {
        File.WriteAllBytes(Path.Combine(_root, "lib.dll"), []);
        string manifest = WritePlugin(typeof(PluginLoader).Assembly.Location, json);

        PluginManifestResolutionResult result = new PluginManifestResolver().Resolve(manifest);

        Assert.False(result.Success);
        Assert.Equal(expected, result.FailureReason);
    }

    [Fact]
    public void ResolvesSharedAssembliesBesideThePluginAssembly()
    {
        File.Copy(typeof(SFSharp.Abstractions.Modules.ISFModule).Assembly.Location, Path.Combine(_root, "Lib.Shared.dll"));
        string manifest = WritePlugin(typeof(PluginLoader).Assembly.Location,
            """{ "id": "p", "version": "1.0.0", "assembly": "p.dll", "sharedAssemblies": [" Lib.Shared "] }""");

        KeyValuePair<string, string> shared = Assert.Single(Resolve(manifest).SharedAssemblyPaths);

        Assert.Equal("Lib.Shared", shared.Key);
        Assert.Equal(Path.Combine(_root, "Lib.Shared.dll"), shared.Value, ignoreCase: true);
    }

    public void Dispose() => Directory.Delete(_root, recursive: true);

    private string WritePlugin(string assemblySource, string manifestJson)
    {
        File.Copy(assemblySource, Path.Combine(_root, "p.dll"), overwrite: true);
        string manifestPath = Path.Combine(_root, PluginManifest.FileName);
        File.WriteAllText(manifestPath, manifestJson);
        return manifestPath;
    }

    private static ResolvedPluginManifest Resolve(string manifestPath)
    {
        PluginManifestResolutionResult result = new PluginManifestResolver().Resolve(manifestPath);
        Assert.True(result.Success, result.Message);
        return result.Manifest!;
    }
}
