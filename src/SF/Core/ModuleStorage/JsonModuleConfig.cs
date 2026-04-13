using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace SFSharp;

public sealed class JsonModuleConfig : IModuleConfig
{
    private readonly IModuleStorage _storage;
    private readonly string _relativePath;
    private readonly JsonWriterOptions _writerOptions;

    public JsonModuleConfig(IModuleStorage storage, string relativePath = "config.json", bool writeIndented = true)
    {
        ArgumentNullException.ThrowIfNull(storage);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        _storage = storage;
        _relativePath = relativePath;
        _writerOptions = new JsonWriterOptions { Indented = writeIndented };
    }

    public bool Exists => _storage.Exists(_relativePath);
    public string Location => _storage.GetFullPath(_relativePath);

    public T Load<T>(JsonTypeInfo<T> typeInfo, Func<T> createDefault)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        ArgumentNullException.ThrowIfNull(createDefault);

        if (!_storage.Exists(_relativePath))
        {
            T fallback = createDefault();
            Save(typeInfo, fallback);
            return fallback;
        }

        using Stream stream = _storage.OpenRead(_relativePath);
        T? value = JsonSerializer.Deserialize(stream, typeInfo);
        return value ?? createDefault();
    }

    public bool TryLoad<T>(JsonTypeInfo<T> typeInfo, out T value)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);
        if (!_storage.Exists(_relativePath))
        {
            value = default!;
            return false;
        }

        using Stream stream = _storage.OpenRead(_relativePath);
        T? deserialized = JsonSerializer.Deserialize(stream, typeInfo);
        if (deserialized is null)
        {
            value = default!;
            return false;
        }

        value = deserialized;
        return true;
    }

    public void Save<T>(JsonTypeInfo<T> typeInfo, T value)
    {
        ArgumentNullException.ThrowIfNull(typeInfo);

        using Stream stream = _storage.OpenWrite(_relativePath);
        using Utf8JsonWriter writer = new(stream, _writerOptions);
        JsonSerializer.Serialize(writer, value, typeInfo);
    }

    public void Delete() => _storage.Delete(_relativePath);
}
