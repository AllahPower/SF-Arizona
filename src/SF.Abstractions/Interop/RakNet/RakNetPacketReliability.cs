namespace SFSharp.Abstractions.Interop.RakNet;

public enum RakNetPacketReliability
{
    Unreliable = 0,
    UnreliableSequenced = 1,
    Reliable = 2,
    ReliableOrdered = 3,
    ReliableSequenced = 4,
}
