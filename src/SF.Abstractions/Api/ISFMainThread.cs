namespace SFSharp.Abstractions;

/// <summary>Executes delegates on the game thread. Awaiting the result does not move the caller's continuation.</summary>
public interface ISFMainThread
{
    bool IsMainThread { get; }
    Task InvokeAsync(Action action, CancellationToken cancellationToken = default);
    Task<T> InvokeAsync<T>(Func<T> action, CancellationToken cancellationToken = default);
}
