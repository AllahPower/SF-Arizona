using System.IO;

public partial class DebugModule
{
    private static readonly string WebDebuggerRootPath = Path.Combine(AppContext.BaseDirectory, "SF", "WebDebugger");
    private static readonly string WebDebuggerWwwRootPath = Path.Combine(WebDebuggerRootPath, "wwwroot");
    private static readonly string WebDebuggerIndexPath = Path.Combine(WebDebuggerWwwRootPath, "index.html");
}
