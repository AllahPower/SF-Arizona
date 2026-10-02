namespace SFSharp.Runtime.Interop.Engine;

/// <summary>Low three bits of <c>CEntity::m_nTypeStatus</c>.</summary>
public enum GtaEntityType : byte
{
    Nothing = 0,
    Building = 1,
    Vehicle = 2,
    Ped = 3,
    Object = 4,
    Dummy = 5,
}
