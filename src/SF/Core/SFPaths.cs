namespace SFSharp.Runtime;

public static class SFPaths
{
    private const string SFSubDirectory = "SF";

    public static string GameDirectory { get; } = ResolveGameDirectory();

    public static string AssetsRoot { get; } = Path.Combine(GameDirectory, SFSubDirectory);

    public static string UserDataRoot { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "GTA San Andreas User Files",
        SFSubDirectory);

    public static string GetModuleAssetsDirectory(string moduleId) => Path.Combine(AssetsRoot, moduleId);

    public static string GetModuleUserDataDirectory(string moduleId) => Path.Combine(UserDataRoot, moduleId);

    private static string ResolveGameDirectory()
    {
        string? processPath = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(processPath))
        {
            string? directory = Path.GetDirectoryName(processPath);
            if (!string.IsNullOrEmpty(directory))
            {
                return directory;
            }
        }

        return AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
