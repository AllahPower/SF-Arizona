namespace SFSharp.Runtime.Modules;

public partial class DebugModule
{
    private const string WwwRootRelativePath = "wwwroot";
    private const string IndexRelativePath = "wwwroot/index.html";

    private string WebDebuggerWwwRootPath => Context.Assets.GetFullPath(WwwRootRelativePath);
    private string WebDebuggerIndexPath => Context.Assets.GetFullPath(IndexRelativePath);

    private bool WebDebuggerAssetsExist => Context.Assets.Exists(IndexRelativePath);
}
