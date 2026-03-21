using SFSharp;

[SFModule("dialog-scraper", "DialogScraper", Category = "Utility", Description = "Selects useful rows from active SA-MP dialogs.", ExecutionModel = ModuleExecutionModel.MainThread, Order = 10)]
public class DialogScraper : SFModuleBase
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            using ModuleLoopScope _ = Context.TrackLoop("dialog-poll");
            if (SF.Keyboard.IsKeyPressed(VK.NUMPAD0))
            {
                RunCore();
            }

            Context.Heartbeat("watching dialogs");
            await Task.Yield();
        }
    }

    private unsafe void RunCore()
    {
        ref readonly CDialog dialog = ref CDialog.Instance;
        if (!dialog.IsActive)
        {
            return;
        }

        DialogStyle style = CDialog.Instance.Style;
        if (style is not (DialogStyle.List or DialogStyle.TabList or DialogStyle.TabListHeaders))
        {
            return;
        }

        Span<string> lines = AnsiString.Decode(CDialog.Instance.Text)!.Split("\n").AsSpan();
        if (style is DialogStyle.TabListHeaders)
        {
            lines = lines.Slice(1);
        }

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.Contains("EXP") || line.Contains("Вирт") || line.Contains("След.страница"))
            {
                dialog.ListBox->SelectedIndex = i;
                Context.IncrementCounter("auto.select");
                Context.ReportActivity($"selected-row:{i}");
                return;
            }
        }
    }
}
