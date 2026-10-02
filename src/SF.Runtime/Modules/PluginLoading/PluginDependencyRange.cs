using System.Globalization;

namespace SFSharp.Runtime.Modules.PluginLoading;

/// <summary>
/// Inclusive supported versions of one dependency plus the version the plugin was built and tested
/// against. <c>max</c> is an exact version or a wildcard such as <c>3.x</c> (every 3.*) or <c>3.4.x</c>.
/// </summary>
public sealed class PluginDependencyRange
{
    private readonly SemanticVersion? _maxExact;
    private readonly int? _maxMajor;
    private readonly int? _maxMinor;

    private PluginDependencyRange(SemanticVersion? min, string? max, SemanticVersion? maxExact, int? maxMajor, int? maxMinor, SemanticVersion? target)
    {
        Min = min;
        Max = max;
        _maxExact = maxExact;
        _maxMajor = maxMajor;
        _maxMinor = maxMinor;
        Target = target;
    }

    /// <summary>No bounds and no target: every version is accepted.</summary>
    public static PluginDependencyRange Empty { get; } = new(null, null, null, null, null, null);

    public SemanticVersion? Min { get; }

    /// <summary>Upper bound as written in the manifest, including wildcards.</summary>
    public string? Max { get; }

    public SemanticVersion? Target { get; }

    public static bool TryParse(string? min, string? max, string? target, out PluginDependencyRange? range, out string? error)
    {
        range = null;
        error = null;

        SemanticVersion? minVersion = null;
        if (!string.IsNullOrWhiteSpace(min) && !SemanticVersion.TryParse(min, out minVersion))
        {
            error = $"min '{min}' is not a SemVer 2.0 version";
            return false;
        }

        SemanticVersion? targetVersion = null;
        if (!string.IsNullOrWhiteSpace(target) && !SemanticVersion.TryParse(target, out targetVersion))
        {
            error = $"target '{target}' is not a SemVer 2.0 version";
            return false;
        }

        SemanticVersion? maxExact = null;
        int? maxMajor = null;
        int? maxMinor = null;
        string? maxText = string.IsNullOrWhiteSpace(max) ? null : max.Trim();
        if (maxText is not null && !TryParseMax(maxText, out maxExact, out maxMajor, out maxMinor))
        {
            error = $"max '{maxText}' is neither a SemVer 2.0 version nor a wildcard like 3.x or 3.4.x";
            return false;
        }

        PluginDependencyRange candidate = new(minVersion, maxText, maxExact, maxMajor, maxMinor, targetVersion);
        error = candidate.Validate();
        if (error is not null)
        {
            return false;
        }

        range = candidate;
        return true;
    }

    /// <summary>Fills bounds the manifest left out: min and target become <paramref name="compiled"/>, max its major.</summary>
    public PluginDependencyRange WithDefaults(SemanticVersion compiled)
    {
        ArgumentNullException.ThrowIfNull(compiled);
        bool hasMax = Max is not null;
        return new PluginDependencyRange(
            Min ?? compiled,
            hasMax ? Max : $"{compiled.Major}.x",
            _maxExact,
            hasMax ? _maxMajor : compiled.Major,
            hasMax ? _maxMinor : null,
            Target ?? compiled);
    }

    public bool Contains(SemanticVersion version)
    {
        ArgumentNullException.ThrowIfNull(version);
        return (Min is null || version >= Min) && IsWithinMax(version);
    }

    public override string ToString()
        => $"min {Min?.ToString() ?? "any"}, max {Max ?? "any"}{(Target is null ? string.Empty : $", target {Target}")}";

    private bool IsWithinMax(SemanticVersion version)
    {
        if (_maxExact is not null)
        {
            return version <= _maxExact;
        }

        if (_maxMajor is not int major)
        {
            return true;
        }

        // Wildcards compare numeric components only, so 3.x also covers 3.9.0-beta but never 4.0.0-alpha.
        if (version.Major != major)
        {
            return version.Major < major;
        }

        return _maxMinor is not int minor || version.Minor <= minor;
    }

    /// <summary>Returns why the bounds contradict each other, or null when they are consistent.</summary>
    public string? Validate()
    {
        if (Min is not null && !IsWithinMax(Min))
        {
            return $"min {Min} is above max {Max}";
        }

        if (Target is not null && Min is not null && Target < Min)
        {
            return $"target {Target} is below min {Min}";
        }

        if (Target is not null && !IsWithinMax(Target))
        {
            return $"target {Target} is above max {Max}";
        }

        return null;
    }

    private static bool TryParseMax(string text, out SemanticVersion? exact, out int? major, out int? minor)
    {
        exact = null;
        major = null;
        minor = null;

        string[] parts = text.Split('.');
        bool lastIsWildcard = parts[^1] is "x" or "X" or "*";
        if (!lastIsWildcard)
        {
            return SemanticVersion.TryParse(text, out exact);
        }

        if (parts.Length is < 2 or > 3 || !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int parsedMajor))
        {
            return false;
        }

        major = parsedMajor;
        if (parts.Length == 3)
        {
            if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int parsedMinor))
            {
                return false;
            }

            minor = parsedMinor;
        }

        return true;
    }
}
