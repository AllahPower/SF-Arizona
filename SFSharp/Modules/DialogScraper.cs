

using SFSharp;

public class DialogScraper : ISFModule
{
    public async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            if (SF.Keyboard.IsKeyPressed(VK.NUMPAD0))
            {
                RunCore();
            }
            await Task.Yield();
        }
    }

    private unsafe void RunCore()
    {
        var dialog = CDialog.Instance;
        if(!dialog.IsActive) return;
        var style = CDialog.Instance.Style;
        if (style is not DialogStyle.List or DialogStyle.TabList or DialogStyle.TabListHeaders) return;

        var lines = AnsiString.Decode(CDialog.Instance.Text)!.Split("\n").AsSpan();
        if (style is DialogStyle.TabListHeaders) lines = lines.Slice(1);

        for(int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if(line.Contains("EXP") || line.Contains("Вирт") || line.Contains("След.страница"))
            {
                dialog.ListBox->SelectedIndex = i;
                return;
            }
        }
    }
}
