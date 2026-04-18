using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SFSharp.Runtime.Modules;

/// <summary>
/// Host-level manifest persisted at <c>&lt;UserDataRoot&gt;\host.json</c>. Stores the user's
/// explicit auto-start intent per module id across sessions, plus diagnostic info about the
/// runtime that last touched the file. Built-ins and plugins share one id space.
/// </summary>
public sealed class SFHostManifest
{
    private const int CurrentSchemaVersion = 1;
    private const string FileName = "host.json";

    private static readonly Lazy<SFHostManifest> _instance = new(static () => new SFHostManifest());
    public static SFHostManifest Instance => _instance.Value;

    private readonly object _sync = new();
    private readonly Dictionary<string, HostModuleEntry> _modules = new(StringComparer.OrdinalIgnoreCase);
    private HostInfo _hostInfo = new();
    private bool _hasLoaded;
    private bool _isReadOnly;

    private SFHostManifest() { }

    private static string FilePath => Path.Combine(SFPaths.UserDataRoot, FileName);

    public void Load()
    {
        lock (_sync)
        {
            if (_hasLoaded)
            {
                return;
            }

            _hasLoaded = true;
            _hostInfo = ResolveHostInfo();

            string path = FilePath;
            if (!File.Exists(path))
            {
                SFLog.Info($"SFHostManifest: '{path}' does not exist, starting with empty intent map");
                return;
            }

            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, $"SFHostManifest: failed to read '{path}', starting empty");
                return;
            }

            if (TryParseTolerant(json, out Dictionary<string, HostModuleEntry> parsed, out int schemaVersion))
            {
                foreach ((string id, HostModuleEntry entry) in parsed)
                {
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }

                    _modules[id] = entry;
                }

                if (schemaVersion > CurrentSchemaVersion)
                {
                    _isReadOnly = true;
                    SFLog.Warn($"SFHostManifest: host.json schemaVersion={schemaVersion} > current={CurrentSchemaVersion}; read-only mode, file will not be overwritten");
                }

                SFLog.Info($"SFHostManifest: loaded {_modules.Count} module intent entries from host.json (schema={schemaVersion})");
                return;
            }

            try
            {
                string backup = path + ".corrupt." + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
                File.Move(path, backup, overwrite: false);
                SFLog.Warn($"SFHostManifest: host.json malformed, backed up to '{backup}', starting empty");
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, "SFHostManifest: failed to back up malformed host.json");
            }
        }
    }

    public bool? TryGetEnabled(string moduleId)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
        {
            return null;
        }

        lock (_sync)
        {
            return _modules.TryGetValue(moduleId, out HostModuleEntry? entry) ? entry.Enabled : null;
        }
    }

    public void Upsert(string moduleId, bool enabled, string source)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
        {
            return;
        }

        lock (_sync)
        {
            string nowUtc = DateTime.UtcNow.ToString("o");
            if (_modules.TryGetValue(moduleId, out HostModuleEntry? existing))
            {
                existing.Enabled = enabled;
                existing.Source = source;
                existing.LastSeenUtc = nowUtc;
            }
            else
            {
                _modules[moduleId] = new HostModuleEntry
                {
                    Enabled = enabled,
                    Source = source,
                    FirstSeenUtc = nowUtc,
                    LastSeenUtc = nowUtc,
                };
            }
        }
    }

    public void SetEnabled(string moduleId, bool enabled)
    {
        if (string.IsNullOrWhiteSpace(moduleId))
        {
            return;
        }

        lock (_sync)
        {
            string nowUtc = DateTime.UtcNow.ToString("o");
            if (_modules.TryGetValue(moduleId, out HostModuleEntry? entry))
            {
                entry.Enabled = enabled;
                entry.LastSeenUtc = nowUtc;
            }
            else
            {
                _modules[moduleId] = new HostModuleEntry
                {
                    Enabled = enabled,
                    FirstSeenUtc = nowUtc,
                    LastSeenUtc = nowUtc,
                };
            }
        }
    }

    public void FlushSync()
    {
        lock (_sync)
        {
            if (_isReadOnly)
            {
                return;
            }

            string path = FilePath;
            string tempPath = path + ".tmp";

            try
            {
                Directory.CreateDirectory(SFPaths.UserDataRoot);

                Dictionary<string, HostModuleEntry> merged = new(StringComparer.OrdinalIgnoreCase);
                if (File.Exists(path))
                {
                    try
                    {
                        string existingJson = File.ReadAllText(path);
                        if (TryParseTolerant(existingJson, out Dictionary<string, HostModuleEntry> diskEntries, out _))
                        {
                            foreach ((string id, HostModuleEntry entry) in diskEntries)
                            {
                                merged[id] = entry;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SFLog.Warn($"SFHostManifest: FlushSync merge read failed: {ex.Message}");
                    }
                }

                foreach ((string id, HostModuleEntry entry) in _modules)
                {
                    merged[id] = entry;
                }

                _hostInfo.LastStartUtc ??= DateTime.UtcNow.ToString("o");

                HostManifestDocument doc = new()
                {
                    SchemaVersion = CurrentSchemaVersion,
                    Host = _hostInfo,
                    Modules = merged,
                };

                string json = JsonSerializer.Serialize(doc, HostManifestJsonContext.Default.HostManifestDocument);
                File.WriteAllText(tempPath, json);
                File.Move(tempPath, path, overwrite: true);
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, "SFHostManifest: FlushSync failed");
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch
                {
                }
            }
        }
    }

    private static bool TryParseTolerant(string json, out Dictionary<string, HostModuleEntry> modules, out int schemaVersion)
    {
        modules = new(StringComparer.OrdinalIgnoreCase);
        schemaVersion = CurrentSchemaVersion;

        JsonDocumentOptions docOptions = new()
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        };

        JsonDocument? document = null;
        try
        {
            document = JsonDocument.Parse(json, docOptions);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }

            if (document.RootElement.TryGetProperty("schemaVersion", out JsonElement sv)
                && sv.ValueKind == JsonValueKind.Number
                && sv.TryGetInt32(out int parsedSv))
            {
                schemaVersion = parsedSv;
            }

            if (!document.RootElement.TryGetProperty("modules", out JsonElement modsEl)
                || modsEl.ValueKind != JsonValueKind.Object)
            {
                return true;
            }

            foreach (JsonProperty prop in modsEl.EnumerateObject())
            {
                if (prop.Value.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                try
                {
                    HostModuleEntry? entry = prop.Value.Deserialize(HostManifestJsonContext.Default.HostModuleEntry);
                    if (entry is null)
                    {
                        continue;
                    }

                    modules[prop.Name] = entry;
                }
                catch (JsonException)
                {
                    SFLog.Warn($"SFHostManifest: skipping malformed entry for module '{prop.Name}'");
                }
            }

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        finally
        {
            document?.Dispose();
        }
    }

    private static HostInfo ResolveHostInfo()
    {
        Assembly asm = typeof(SFHostManifest).Assembly;
        string? informational = asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        string? fileVersion = asm.GetName().Version?.ToString();
        string? commit = null;

        if (!string.IsNullOrWhiteSpace(informational))
        {
            int plus = informational.IndexOf('+');
            if (plus >= 0 && plus < informational.Length - 1)
            {
                commit = informational[(plus + 1)..];
            }
        }

        return new HostInfo
        {
            Version = fileVersion,
            InformationalVersion = informational,
            Commit = commit,
            AssemblyName = asm.GetName().Name,
            LastStartUtc = DateTime.UtcNow.ToString("o"),
        };
    }
}

public sealed class HostManifestDocument
{
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; } = 1;

    [JsonPropertyName("host")]
    public HostInfo? Host { get; set; }

    [JsonPropertyName("modules")]
    public Dictionary<string, HostModuleEntry> Modules { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class HostInfo
{
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("informationalVersion")]
    public string? InformationalVersion { get; set; }

    [JsonPropertyName("commit")]
    public string? Commit { get; set; }

    [JsonPropertyName("assemblyName")]
    public string? AssemblyName { get; set; }

    [JsonPropertyName("lastStartUtc")]
    public string? LastStartUtc { get; set; }
}

public sealed class HostModuleEntry
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("firstSeenUtc")]
    public string? FirstSeenUtc { get; set; }

    [JsonPropertyName("lastSeenUtc")]
    public string? LastSeenUtc { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    AllowTrailingCommas = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(HostManifestDocument))]
[JsonSerializable(typeof(HostModuleEntry))]
[JsonSerializable(typeof(HostInfo))]
internal sealed partial class HostManifestJsonContext : JsonSerializerContext;
