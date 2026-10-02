namespace SFSharp.Runtime.Diagnostics;

/// <summary>Last-resort sink for unhandled runtime exceptions: the SF log plus a chat line when the chat is up.</summary>
public sealed class ExceptionReporter
{
    public void Report(Exception ex)
    {
        SFLog.Error(ex, "Unhandled library exception");

        // Early-loading exceptions can arrive before samp.dll is mapped, where resolving CChat would read a bogus address.
        if (!ModuleResolver.IsModuleLoaded("samp.dll"))
        {
            return;
        }

        try
        {
            CChat.Instance.AddEntry(EntryType.Chat, $"{ex.GetType()}: {ex.Message}", null, 0xFFFFFFFF, 0);
        }
        catch (Exception chatEx)
        {
            SFLog.Warn($"ProcessException fallback skipped chat output: {chatEx.GetType().Name}: {chatEx.Message}");
        }
    }

    public void Observe(Task task, string source)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        task.ContinueWith(static (completed, state) =>
        {
            (ExceptionReporter reporter, string taskSource) = ((ExceptionReporter, string))state!;
            if (completed.IsFaulted && completed.Exception is not null)
            {
                SFLog.Error(completed.Exception.GetBaseException(), $"Unhandled task exception from {taskSource}");
                reporter.Report(completed.Exception.GetBaseException());
            }
        }, (this, source), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
    }
}
