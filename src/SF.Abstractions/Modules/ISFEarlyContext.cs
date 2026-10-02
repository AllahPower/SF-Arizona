using Microsoft.Extensions.Logging;

namespace SFSharp.Abstractions.Modules;

/// <summary>Services available to an <see cref="ISFEarlyModule"/> for the lifetime of its plugin.</summary>
public interface ISFEarlyContext
{
    /// <summary>Id of the plugin that owns the early module.</summary>
    string PluginId { get; }

    /// <summary>Logger scoped to the plugin and early module type. Thread-safe.</summary>
    ILogger Log { get; }

    /// <summary>
    /// Game start-up stages. Subscriptions made through this instance are removed when the plugin unloads.
    /// </summary>
    ISFGameLoading Loading { get; }

    /// <summary>Takes ownership of <paramref name="disposable"/>; it is disposed when the plugin unloads. Thread-safe.</summary>
    IDisposable RegisterDisposable(IDisposable disposable);
}
