using SF.Fixture.SharedLib;
using SFSharp.Abstractions.Modules;

namespace SF.Fixture.Consumer;

public sealed class ConsumerEarlyModule : ISFEarlyModule
{
    public void OnGameLoading(ISFEarlyContext context) => SharedCounter.Increment();
}
