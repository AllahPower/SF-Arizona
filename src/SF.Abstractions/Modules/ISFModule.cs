namespace SFSharp.Abstractions.Modules;

/// <summary>A module run. The returned task represents its entire lifetime.</summary>
public interface ISFModule
{
    /// <summary>Run until completion or context cancellation. Do not leave unowned background work.</summary>
    Task RunAsync(IModuleContext context);
}
