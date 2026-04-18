namespace SFSharp.Runtime.Game.World;

public readonly unsafe record struct SFObjectMaterialSlot(SFObject Object, int Slot)
{
    private CObject* Native => Object.Native;

    public int Type => Native is null ? 0 : Native->GetMaterialType(Slot);
    public uint Color => Native is null ? 0u : Native->GetMaterialColor(Slot);
    public bool Exists => Type != 0;
    public nint SpritePointer => Native is null ? 0 : Native->GetMaterialSpritePointer(Slot);
    public bool TextTextureCreated => Native is not null && Native->IsMaterialTextTextureCreated(Slot);
    public nint TextPointer => Native is null ? 0 : Native->GetMaterialTextPointer(Slot);
    public nint BackgroundTexturePointer => Native is null ? 0 : Native->GetMaterialBackgroundTexturePointer(Slot);
    public nint TexturePointer => Native is null ? 0 : Native->GetMaterialTexturePointer(Slot);

    public SFObjectMaterialTextInfo TextInfo
    {
        get
        {
            if (Native is null)
            {
                return default;
            }

            CObjectMaterialTextInfo info = Native->GetMaterialTextInfo(Slot);
            return new(
                info.MaterialIndex,
                info.MaterialSize,
                info.GetFontName(),
                info.FontSize,
                info.Bold != 0,
                info.FontColor,
                info.BackgroundColor,
                info.Align);
        }
    }
}
