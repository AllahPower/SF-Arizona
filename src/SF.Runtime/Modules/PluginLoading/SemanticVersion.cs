using System.Globalization;

namespace SFSharp.Runtime.Modules.PluginLoading;

/// <summary>SemVer 2.0 version. Build metadata is kept for display and ignored by comparisons.</summary>
public sealed class SemanticVersion : IComparable<SemanticVersion>, IEquatable<SemanticVersion>
{
    private readonly string[] _prerelease;

    public SemanticVersion(int major, int minor, int patch, IReadOnlyList<string>? prerelease = null, string? buildMetadata = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(major);
        ArgumentOutOfRangeException.ThrowIfNegative(minor);
        ArgumentOutOfRangeException.ThrowIfNegative(patch);
        Major = major;
        Minor = minor;
        Patch = patch;
        _prerelease = prerelease is null ? [] : [.. prerelease];
        BuildMetadata = buildMetadata;
    }

    public int Major { get; }

    public int Minor { get; }

    public int Patch { get; }

    public IReadOnlyList<string> Prerelease => _prerelease;

    public string? BuildMetadata { get; }

    public bool IsPrerelease => _prerelease.Length != 0;

    /// <summary>Major.Minor.Patch without prerelease or build metadata.</summary>
    public SemanticVersion CoreVersion => IsPrerelease || BuildMetadata is not null ? new(Major, Minor, Patch) : this;

    public static SemanticVersion FromAssemblyVersion(Version version)
    {
        ArgumentNullException.ThrowIfNull(version);
        return new(version.Major, version.Minor, Math.Max(version.Build, 0));
    }

    public static SemanticVersion Parse(string text)
        => TryParse(text, out SemanticVersion? version) ? version! : throw new FormatException($"'{text}' is not a SemVer 2.0 version.");

    public static bool TryParse(string? text, out SemanticVersion? version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string value = text.Trim();
        string? buildMetadata = null;
        int plus = value.IndexOf('+');
        if (plus >= 0)
        {
            buildMetadata = value[(plus + 1)..];
            value = value[..plus];
            if (!AreValidIdentifiers(buildMetadata.Split('.'), numericLeadingZeroAllowed: true))
            {
                return false;
            }
        }

        string[] prerelease = [];
        int dash = value.IndexOf('-');
        if (dash >= 0)
        {
            prerelease = value[(dash + 1)..].Split('.');
            value = value[..dash];
            if (!AreValidIdentifiers(prerelease, numericLeadingZeroAllowed: false))
            {
                return false;
            }
        }

        string[] core = value.Split('.');
        if (core.Length != 3
            || !TryParseNumber(core[0], out int major)
            || !TryParseNumber(core[1], out int minor)
            || !TryParseNumber(core[2], out int patch))
        {
            return false;
        }

        version = new SemanticVersion(major, minor, patch, prerelease, buildMetadata);
        return true;
    }

    public int CompareTo(SemanticVersion? other)
    {
        if (other is null)
        {
            return 1;
        }

        int result = Major.CompareTo(other.Major);
        if (result == 0) result = Minor.CompareTo(other.Minor);
        if (result == 0) result = Patch.CompareTo(other.Patch);
        if (result != 0)
        {
            return result;
        }

        // A release ranks above any prerelease of the same core version.
        if (_prerelease.Length == 0 || other._prerelease.Length == 0)
        {
            return other._prerelease.Length.CompareTo(_prerelease.Length);
        }

        for (int i = 0; i < Math.Min(_prerelease.Length, other._prerelease.Length); i++)
        {
            result = ComparePrereleaseIdentifier(_prerelease[i], other._prerelease[i]);
            if (result != 0)
            {
                return result;
            }
        }

        return _prerelease.Length.CompareTo(other._prerelease.Length);
    }

    public bool Equals(SemanticVersion? other) => CompareTo(other) == 0;

    public override bool Equals(object? obj) => obj is SemanticVersion other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, string.Join('.', _prerelease));

    public override string ToString()
    {
        string value = $"{Major}.{Minor}.{Patch}";
        if (IsPrerelease)
        {
            value += "-" + string.Join('.', _prerelease);
        }

        return BuildMetadata is null ? value : $"{value}+{BuildMetadata}";
    }

    public static bool operator ==(SemanticVersion? left, SemanticVersion? right) => left is null ? right is null : left.Equals(right);

    public static bool operator !=(SemanticVersion? left, SemanticVersion? right) => !(left == right);

    public static bool operator <(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) < 0;

    public static bool operator >(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) > 0;

    public static bool operator <=(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) <= 0;

    public static bool operator >=(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) >= 0;

    private static int ComparePrereleaseIdentifier(string left, string right)
    {
        bool leftNumeric = int.TryParse(left, NumberStyles.None, CultureInfo.InvariantCulture, out int leftNumber);
        bool rightNumeric = int.TryParse(right, NumberStyles.None, CultureInfo.InvariantCulture, out int rightNumber);
        if (leftNumeric && rightNumeric)
        {
            return leftNumber.CompareTo(rightNumber);
        }

        // Numeric identifiers rank below alphanumeric ones.
        if (leftNumeric != rightNumeric)
        {
            return leftNumeric ? -1 : 1;
        }

        return string.CompareOrdinal(left, right);
    }

    private static bool TryParseNumber(string text, out int value)
    {
        value = 0;
        return text.Length != 0
            && (text.Length == 1 || text[0] != '0')
            && int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    private static bool AreValidIdentifiers(string[] identifiers, bool numericLeadingZeroAllowed)
    {
        foreach (string identifier in identifiers)
        {
            if (identifier.Length == 0 || !identifier.All(static c => char.IsAsciiLetterOrDigit(c) || c == '-'))
            {
                return false;
            }

            if (!numericLeadingZeroAllowed && identifier.Length > 1 && identifier[0] == '0' && identifier.All(char.IsAsciiDigit))
            {
                return false;
            }
        }

        return true;
    }
}
