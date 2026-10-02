using System.Reflection;
using SFSharp.Runtime.Diagnostics;

namespace SF.Network.Tests;

public sealed class RuntimeBuildInfoTests
{
    [Fact]
    public void SplitsInformationalVersionIntoVersionAndShortCommit()
    {
        string informational = typeof(RuntimeBuildInfo).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

        Assert.DoesNotContain('+', RuntimeBuildInfo.Version);
        Assert.StartsWith(RuntimeBuildInfo.Version, informational);
        if (RuntimeBuildInfo.Commit is { } commit)
        {
            Assert.Equal(7, commit.Length);
            Assert.StartsWith($"{RuntimeBuildInfo.Version}+{commit}", informational);
        }
    }

    [Fact]
    public void ReadsEmbeddedBuildTimestamp()
    {
        Assert.NotNull(RuntimeBuildInfo.BuildTimestamp);
        Assert.InRange(RuntimeBuildInfo.BuildTimestamp!.Value, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow.AddMinutes(5));
    }
}
