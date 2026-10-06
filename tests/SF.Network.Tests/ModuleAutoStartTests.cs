using SFSharp.Runtime.Modules.Hosting;

namespace SF.Network.Tests;

public sealed class ModuleAutoStartTests
{
    [Theory]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, true, false)]
    [InlineData(null, true, false, true)]
    [InlineData(null, false, true, false)]
    [InlineData(null, null, true, true)]
    [InlineData(null, null, false, false)]
    public void SavedIntentOverridesPluginAndModuleDefaults(
        bool? storedIntent,
        bool? enabledOnStart,
        bool defaultEnabled,
        bool expected)
    {
        Assert.Equal(expected, SFModuleContainer.ResolveAutoStartEnabled(storedIntent, enabledOnStart, defaultEnabled));
    }
}
