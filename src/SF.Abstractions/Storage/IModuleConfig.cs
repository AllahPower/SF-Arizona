using System.Text.Json.Serialization.Metadata;

namespace SFSharp.Abstractions.Storage;

public interface IModuleConfig
{
    bool Exists { get; }
    string Location { get; }

    T Load<T>(JsonTypeInfo<T> typeInfo, Func<T> createDefault);
    bool TryLoad<T>(JsonTypeInfo<T> typeInfo, out T value);
    void Save<T>(JsonTypeInfo<T> typeInfo, T value);
    void Delete();
}
