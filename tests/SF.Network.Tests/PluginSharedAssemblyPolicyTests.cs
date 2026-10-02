using SFSharp.Abstractions.Modules;
using SFSharp.Runtime.Modules.PluginLoading;
using System.Reflection;
using System.Runtime.Loader;

namespace SF.Network.Tests;

public sealed class PluginSharedAssemblyPolicyTests
{
    [Fact]
    public void HostLibraryResolvesToDefaultContext()
    {
        AssemblyName requested = typeof(ISFModule).Assembly.GetName();

        Assert.True(PluginSharedAssemblyPolicy.IsHostLibrary(requested));
        Assembly resolved = PluginSharedAssemblyPolicy.ResolveHostLibrary(requested);

        Assert.Same(typeof(ISFModule).Assembly, resolved);
        Assert.Same(AssemblyLoadContext.Default, AssemblyLoadContext.GetLoadContext(resolved));
    }

    [Fact]
    public void ProtocolIsSharedAsTheInstanceTheHostUses()
    {
        Assert.True(PluginSharedAssemblyPolicy.IsShared("SF.Protocol"));
        Assert.True(PluginSharedAssemblyPolicy.TryResolveLoadedAssembly("SF.Protocol", out Assembly? resolved));
        Assert.Same(typeof(SFSharp.Protocol.Rpc.SampRpc).Assembly, resolved);
    }

    [Theory]
    [InlineData("../SF.Abstractions")]
    [InlineData("Missing.Shared.Library")]
    public void MissingOrInvalidHostLibraryIsNotShared(string name)
    {
        Assert.False(PluginSharedAssemblyPolicy.IsHostLibrary(new AssemblyName { Name = name }));
    }
}
