using SFSharp;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using unsafe CreateDelegate = delegate* unmanaged[Thiscall]<CTextDrawPool*, int, CTextDrawTransmit*, byte*, nint>;
using unsafe DeleteDelegate = delegate* unmanaged[Thiscall]<CTextDrawPool*, ushort, void>;
using unsafe DrawDelegate = delegate* unmanaged[Thiscall]<CTextDrawPool*, void>;

[StructLayout(LayoutKind.Explicit, Size = 18432, Pack = 1)]
public unsafe ref struct CTextDrawPool
{
    private static CTextDrawPool* CurrentInstance => CNetGame.Instance.GetTextDrawPool();
    public static ref readonly CTextDrawPool Instance => ref *RequireInstance();

    private static readonly DeleteDelegate _delete = (DeleteDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CTextDrawPool.Delete);
    private static readonly DrawDelegate _draw = (DrawDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CTextDrawPool.Draw);
    private static readonly CreateDelegate _create = (CreateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CTextDrawPool.Create);

    public bool IsValidId(ushort textDrawId) => textDrawId < SampOffsets.CTextDrawPool.TotalTextDraws;

    public CTextDraw* Get(ushort textDrawId)
    {
        CTextDrawPool* instance = RequireInstance();
        if (!IsValidId(textDrawId))
        {
            return null;
        }

        return GetObjectArray(instance)[textDrawId];
    }

    public bool TryGet(ushort textDrawId, out CTextDraw* textDraw)
    {
        textDraw = null;
        if (!DoesExist(textDrawId))
        {
            return false;
        }

        textDraw = Get(textDrawId);
        return textDraw != null;
    }

    public bool DoesExist(ushort textDrawId)
    {
        CTextDrawPool* instance = RequireInstance();
        return IsValidId(textDrawId) && GetOccupancyArray(instance)[textDrawId] != 0;
    }

    public CTextDraw* Create(int textDrawId, in CTextDrawTransmit transmit, string text)
    {
        CTextDrawTransmit copy = transmit;
        using AnsiString textAnsi = AnsiString.Encode(text);
        return (CTextDraw*)_create(RequireInstance(), textDrawId, &copy, textAnsi);
    }

    public void Delete(ushort textDrawId)
    {
        _delete(RequireInstance(), textDrawId);
    }

    public unsafe ushort[] GetExistingIds()
    {
        CTextDrawPool* instance = RequireInstance();
        int* occupancy = GetOccupancyArray(instance);
        List<ushort> ids = [];
        for (ushort textDrawId = 0; textDrawId < SampOffsets.CTextDrawPool.TotalTextDraws; textDrawId++)
        {
            if (occupancy[textDrawId] != 0)
            {
                ids.Add(textDrawId);
            }
        }

        return [.. ids];
    }

    public void Draw()
    {
        _draw(RequireInstance());
    }

    private static int* GetOccupancyArray(CTextDrawPool* instance)
    {
        return (int*)((byte*)instance + SampOffsets.CTextDrawPool.OccupancyArray);
    }

    private static CTextDraw** GetObjectArray(CTextDrawPool* instance)
    {
        return (CTextDraw**)((byte*)instance + SampOffsets.CTextDrawPool.ObjectArray);
    }

    private static CTextDrawPool* RequireInstance()
    {
        CTextDrawPool* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CTextDrawPool instance is not available.");
        }

        return instance;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 2518, Pack = 1)]
public unsafe struct CTextDraw
{
    [FieldOffset(0x000)]
    public fixed byte Text[801];

    [FieldOffset(0x321)]
    public fixed byte String[1602];

    [FieldOffset(0x963)]
    public CTextDrawData Data;

    private static readonly delegate* unmanaged[Thiscall]<CTextDraw*, byte*, void> _setText = (delegate* unmanaged[Thiscall]<CTextDraw*, byte*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CTextDraw.SetText);
    private static readonly delegate* unmanaged[Thiscall]<CTextDraw*, void> _draw = (delegate* unmanaged[Thiscall]<CTextDraw*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CTextDraw.Draw);

    public string? GetText()
    {
        fixed (byte* text = Text)
        {
            return NativeString.Decode(text, 801);
        }
    }

    public string? GetString()
    {
        fixed (byte* text = String)
        {
            return NativeString.Decode(text, 1602);
        }
    }

    public void SetText(string text)
    {
        using AnsiString textAnsi = AnsiString.Encode(text);
        _setText((CTextDraw*)Unsafe.AsPointer(ref this), textAnsi);
    }

    public void Draw()
    {
        _draw((CTextDraw*)Unsafe.AsPointer(ref this));
    }
}

[StructLayout(LayoutKind.Sequential, Size = 63, Pack = 1)]
public struct CTextDrawTransmit
{
    public byte Flags;
    public float LetterWidth;
    public float LetterHeight;
    public uint LetterColor;
    public float BoxWidth;
    public float BoxHeight;
    public uint BoxColor;
    public byte Shadow;
    public byte Outline;
    public uint BackgroundColor;
    public byte Style;
    public byte Unknown;
    public float X;
    public float Y;
    public ushort Model;
    public System.Numerics.Vector3 Rotation;
    public float Zoom;
    public ushort Color0;
    public ushort Color1;
}

[StructLayout(LayoutKind.Sequential, Size = 115, Pack = 1)]
public struct CTextDrawData
{
    public float LetterWidth;
    public float LetterHeight;
    public uint LetterColor;
    public byte Unknown;
    public byte Center;
    public byte Box;
    public float BoxSizeX;
    public float BoxSizeY;
    public uint BoxColor;
    public byte Proportional;
    public uint BackgroundColor;
    public byte Shadow;
    public byte Outline;
    public byte Left;
    public byte Right;
    public int Style;
    public float X;
    public float Y;
    public ulong Padding0;
    public uint Field99B;
    public uint Field99F;
    public int Index;
    public byte Selectable;
    public ushort Model;
    public System.Numerics.Vector3 Rotation;
    public float Zoom;
    public ushort Color0;
    public ushort Color1;
    public byte TextContainsKeys;
    public byte DrawnThisFrame;
    public byte IsSelected;
    public uint ComputedLeft;
    public uint ComputedRight;
    public uint ComputedTop;
    public uint ComputedBottom;
    public byte ColorIfSelected;
}
