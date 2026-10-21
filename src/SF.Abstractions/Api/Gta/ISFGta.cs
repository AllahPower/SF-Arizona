namespace SFSharp.Abstractions.Gta;

/// <summary>Local GTA engine services, without network IDs or SA-MP pointers.</summary>
public interface ISFGta
{
    ISFEntities Entities { get; }
    ISFCamera Camera { get; }
    ISFGameClock Clock { get; }
    ISFWorld World { get; }
}
