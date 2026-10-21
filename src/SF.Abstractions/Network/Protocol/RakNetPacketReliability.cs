namespace SFSharp.Abstractions.Network.Protocol;

public enum RakNetPacketReliability
{
    Unreliable = 0,
    UnreliableSequenced = 1,
    Reliable = 2,
    ReliableOrdered = 3,
    ReliableSequenced = 4,
}
