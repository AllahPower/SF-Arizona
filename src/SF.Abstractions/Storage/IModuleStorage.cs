namespace SFSharp.Abstractions.Storage;

public interface IModuleStorage
{
    string Root { get; }
    bool IsReadOnly { get; }

    bool Exists(string relativePath);
    string GetFullPath(string relativePath);

    Stream OpenRead(string relativePath);
    Stream OpenWrite(string relativePath);

    byte[] ReadAllBytes(string relativePath);
    string ReadAllText(string relativePath);
    string[] ReadAllLines(string relativePath);

    void WriteAllBytes(string relativePath, ReadOnlySpan<byte> contents);
    void WriteAllText(string relativePath, string contents);
    void WriteAllLines(string relativePath, IEnumerable<string> lines);

    bool Delete(string relativePath);
    IEnumerable<string> EnumerateFiles(string relativeDirectory = "", string searchPattern = "*", bool recursive = false);
}
