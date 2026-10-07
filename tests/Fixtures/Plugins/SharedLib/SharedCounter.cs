using SF.Fixture.SharedLibDep;

namespace SF.Fixture.SharedLib;

/// <summary>Static state that only adds up across plugins when they bind to one shared copy.</summary>
public static class SharedCounter
{
    private static int _value;

    public static int Value => Volatile.Read(ref _value);

    public static string Increment() => $"{Interlocked.Increment(ref _value)}:{DependencyMarker.Tag}";
}
