namespace SFSharp.Abstractions.Samp;

/// <summary>A requested native SA-MP binding has not been verified for this client revision.</summary>
public sealed class SFSampFeatureUnavailableException : NotSupportedException
{
    public SFSampVersion Version { get; }
    public string Feature { get; }

    public SFSampFeatureUnavailableException(SFSampVersion version, string feature)
        : base($"SA-MP feature '{feature}' is unavailable on {version}: no verified native binding.")
    {
        Version = version;
        Feature = feature;
    }
}
