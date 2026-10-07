using SF.Fixture.SharedLib;
using SFSharp.Abstractions.Modules;

namespace SF.Fixture.Provider;

public sealed class ProviderEarlyModule : ISFEarlyModule
{
    public void OnGameLoading(ISFEarlyContext context) => SharedCounter.Increment();
}
