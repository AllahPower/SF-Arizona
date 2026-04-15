using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public interface IParsedRpc
{
    ERpcId ERpcId { get; }
    string Name { get; }
    string? Detail { get; }
}

public interface IParsedIncomingRpc : IParsedRpc
{
}

public interface IParsedOutgoingRpc : IParsedRpc
{
}
