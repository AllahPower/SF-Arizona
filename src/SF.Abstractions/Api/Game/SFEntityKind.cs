namespace SFSharp.Abstractions.Game;

/// <summary>GTA world entity pool. Values match the engine entity type in <c>CEntity::m_nTypeStatus</c>.</summary>
public enum SFEntityKind : byte
{
    Vehicle = 2,
    Ped = 3,
    Object = 4,
}
