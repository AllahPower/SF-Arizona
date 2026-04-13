namespace SFSharp;

public sealed class FileSystemModuleStorage : IModuleStorage
{
    private readonly string _root;

    public FileSystemModuleStorage(string rootDirectory, bool isReadOnly = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        _root = Path.GetFullPath(rootDirectory);
        IsReadOnly = isReadOnly;
    }

    public string Root => _root;
    public bool IsReadOnly { get; }

    public bool Exists(string relativePath) => File.Exists(Resolve(relativePath));

    public string GetFullPath(string relativePath) => Resolve(relativePath);

    public Stream OpenRead(string relativePath) => File.OpenRead(Resolve(relativePath));

    public Stream OpenWrite(string relativePath)
    {
        EnsureWritable();
        string full = Resolve(relativePath);
        EnsureDirectory(full);
        return new FileStream(full, FileMode.Create, FileAccess.Write, FileShare.Read);
    }

    public byte[] ReadAllBytes(string relativePath) => File.ReadAllBytes(Resolve(relativePath));
    public string ReadAllText(string relativePath) => File.ReadAllText(Resolve(relativePath));
    public string[] ReadAllLines(string relativePath) => File.ReadAllLines(Resolve(relativePath));

    public void WriteAllBytes(string relativePath, ReadOnlySpan<byte> contents)
    {
        EnsureWritable();
        string full = Resolve(relativePath);
        EnsureDirectory(full);
        using FileStream stream = new(full, FileMode.Create, FileAccess.Write, FileShare.Read);
        stream.Write(contents);
    }

    public void WriteAllText(string relativePath, string contents)
    {
        EnsureWritable();
        string full = Resolve(relativePath);
        EnsureDirectory(full);
        File.WriteAllText(full, contents);
    }

    public void WriteAllLines(string relativePath, IEnumerable<string> lines)
    {
        EnsureWritable();
        string full = Resolve(relativePath);
        EnsureDirectory(full);
        File.WriteAllLines(full, lines);
    }

    public bool Delete(string relativePath)
    {
        EnsureWritable();
        string full = Resolve(relativePath);
        if (!File.Exists(full))
        {
            return false;
        }

        File.Delete(full);
        return true;
    }

    public IEnumerable<string> EnumerateFiles(string relativeDirectory = "", string searchPattern = "*", bool recursive = false)
    {
        string directory = string.IsNullOrEmpty(relativeDirectory) ? _root : Resolve(relativeDirectory);
        if (!Directory.Exists(directory))
        {
            return [];
        }

        SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return Directory.EnumerateFiles(directory, searchPattern, option)
            .Select(full => Path.GetRelativePath(_root, full).Replace('\\', '/'));
    }

    private string Resolve(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        if (Path.IsPathRooted(relativePath))
        {
            throw new ArgumentException($"Module storage paths must be relative: '{relativePath}'.", nameof(relativePath));
        }

        string combined = Path.GetFullPath(Path.Combine(_root, relativePath));
        string rootWithSep = _root.EndsWith(Path.DirectorySeparatorChar) ? _root : _root + Path.DirectorySeparatorChar;
        if (!combined.StartsWith(rootWithSep, StringComparison.OrdinalIgnoreCase) && !string.Equals(combined, _root, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException($"Path '{relativePath}' escapes storage root '{_root}'.");
        }

        return combined;
    }

    private void EnsureWritable()
    {
        if (IsReadOnly)
        {
            throw new InvalidOperationException($"Storage at '{_root}' is read-only.");
        }
    }

    private static void EnsureDirectory(string fullFilePath)
    {
        string? directory = Path.GetDirectoryName(fullFilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
