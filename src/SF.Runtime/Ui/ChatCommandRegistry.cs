namespace SFSharp.Runtime.Ui;

internal sealed class ChatCommandRegistry
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, Registration> _commands = [];

    public IDisposable Register(string name, Action<string?> handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(handler);
        var registration = new Registration(this, name, handler);
        lock (_sync) _commands.Add(name, registration);
        return registration;
    }

    public bool TryInvoke(string name, string? args)
    {
        Registration? registration;
        lock (_sync) _commands.TryGetValue(name, out registration);
        if (registration is null) return false;
        registration.Invoke(args);
        return true;
    }

    private void Remove(string name, Registration registration)
    {
        lock (_sync)
        {
            if (_commands.TryGetValue(name, out Registration? current) && ReferenceEquals(current, registration))
                _commands.Remove(name);
        }
    }

    private sealed class Registration(ChatCommandRegistry owner, string name, Action<string?> handler) : IDisposable
    {
        private ChatCommandRegistry? _owner = owner;
        private Action<string?>? _handler = handler;

        public void Invoke(string? args) => Volatile.Read(ref _handler)?.Invoke(args);

        public void Dispose()
        {
            ChatCommandRegistry? registry = Interlocked.Exchange(ref _owner, null);
            if (registry is null) return;
            Interlocked.Exchange(ref _handler, null);
            registry.Remove(name, this);
        }
    }
}
