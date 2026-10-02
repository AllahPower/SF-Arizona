namespace SFSharp.Runtime.Interop.Engine;

/// <summary>Non-virtual gta_sa.exe functions used by the entity facade. Main thread only.</summary>
public static unsafe class GtaFunctions
{
    private static readonly delegate* unmanaged[Thiscall]<GtaPlaceable*, float, void> _setHeading = (delegate* unmanaged[Thiscall]<GtaPlaceable*, float, void>)ModuleResolver.GetGameAddress(GtaOffsets.CPlaceable.SetHeadingRva);

    /// <summary><c>CPlaceable::SetHeading</c>: rotates the matrix about Z, or stores the simple angle.</summary>
    public static void SetHeading(GtaPlaceable* placeable, float radians)
    {
        _setHeading(placeable, radians);
    }
}
