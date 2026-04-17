using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop;

[StructLayout(LayoutKind.Explicit, Size = 4508, Pack = 1)]
public unsafe struct CObject
{
    public const int MaterialSlotCount = 16;
    private const int MaterialBaseOffset = 0x217;
    private const int MaterialSpritePointersOffset = 0x000;
    private const int MaterialColorsOffset = 0x040;
    private const int MaterialTypesOffset = 0x0C4;
    private const int MaterialTextCreatedOffset = 0x104;
    private const int MaterialTextInfoOffset = 0x145;
    private const int MaterialTextInfoSize = 215;
    private const int MaterialTextPointersOffset = 0xEB4;
    private const int MaterialBackgroundTexturePointersOffset = 0xEF4;
    private const int MaterialTexturePointersOffset = 0xF34;

    [FieldOffset(0x000)]
    private CEntity _entity;

    [FieldOffset(0x04E)]
    private int _model;

    [FieldOffset(0x053)]
    private byte _dontCollideWithCamera;

    [FieldOffset(0x054)]
    private float _drawDistance;

    [FieldOffset(0x058)]
    private float _unknownDistanceValue;

    [FieldOffset(0x05C)]
    private Vector3 _position;

    [FieldOffset(0x068)]
    private float _distanceToCamera;

    [FieldOffset(0x06C)]
    private byte _drawLast;

    [FieldOffset(0x0BE)]
    private ushort _attachedToVehicleId;

    [FieldOffset(0x0C0)]
    private ushort _attachedToObjectId;

    [FieldOffset(0x0C2)]
    private Vector3 _attachOffset;

    [FieldOffset(0x0CE)]
    private Vector3 _attachRotation;

    [FieldOffset(0x0DA)]
    private byte _attachmentSyncFlag;

    [FieldOffset(0x0DB)]
    private CMatrix _targetMatrix;

    [FieldOffset(0x1AF)]
    private byte _moving;

    [FieldOffset(0x1B0)]
    private float _movementSpeed;

    [FieldOffset(0x217)]
    private CObjectMaterial _material;

    [FieldOffset(0x118B)]
    private int _hasCustomMaterial;

    private static readonly delegate* unmanaged[Thiscall]<CObject*, CMatrix*, float> _getDistance = (delegate* unmanaged[Thiscall]<CObject*, CMatrix*, float>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.GetDistance);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, void> _stop = (delegate* unmanaged[Thiscall]<CObject*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.Stop);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, Vector3*, void> _setRotation = (delegate* unmanaged[Thiscall]<CObject*, Vector3*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.SetRotation);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, ushort, Vector3*, Vector3*, void> _setAttachedToVehicle = (delegate* unmanaged[Thiscall]<CObject*, ushort, Vector3*, Vector3*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.SetAttachedToVehicle);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, ushort, Vector3*, Vector3*, byte, void> _setAttachedToObject = (delegate* unmanaged[Thiscall]<CObject*, ushort, Vector3*, Vector3*, byte, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.SetAttachedToObject);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, CVehicle*, void> _attachToVehicle = (delegate* unmanaged[Thiscall]<CObject*, CVehicle*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.AttachToVehicle);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, CObject*, void> _attachToObject = (delegate* unmanaged[Thiscall]<CObject*, CObject*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.AttachToObject);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, Vector3, void> _rotate = (delegate* unmanaged[Thiscall]<CObject*, Vector3, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.Rotate);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, int> _attachedToMovingEntity = (delegate* unmanaged[Thiscall]<CObject*, int>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.AttachedToMovingEntity);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, int, int, byte*, byte*, uint, void> _setMaterial = (delegate* unmanaged[Thiscall]<CObject*, int, int, byte*, byte*, uint, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.SetMaterial);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, int, byte*, byte, byte*, byte, byte, uint, uint, byte, void> _setMaterialText = (delegate* unmanaged[Thiscall]<CObject*, int, byte*, byte, byte*, byte, byte, uint, uint, byte, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.SetMaterialText);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, int, int*, int*, byte> _getMaterialSize = (delegate* unmanaged[Thiscall]<CObject*, int, int*, int*, byte>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.GetMaterialSize);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, void> _shutdownMaterialText = (delegate* unmanaged[Thiscall]<CObject*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.ShutdownMaterialText);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, void> _render = (delegate* unmanaged[Thiscall]<CObject*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.Render);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, float, void> _process = (delegate* unmanaged[Thiscall]<CObject*, float, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.Process);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, void> _constructMaterialText = (delegate* unmanaged[Thiscall]<CObject*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.ConstructMaterialText);
    private static readonly delegate* unmanaged[Thiscall]<CObject*, void> _draw = (delegate* unmanaged[Thiscall]<CObject*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObject.Draw);

    public CEntity Entity => _entity;
    public int Model => _model;
    public bool DontCollideWithCamera => _dontCollideWithCamera != 0;
    public float DrawDistance => _drawDistance;
    public float UnknownDistanceValue => _unknownDistanceValue;
    public Vector3 Position => _position;
    public float DistanceToCamera => _distanceToCamera;
    public bool DrawLast => _drawLast != 0;
    public Vector3 Rotation => GetEntityPointer()->GetEulerInverted();
    public ushort AttachedToVehicleId => _attachedToVehicleId;
    public ushort AttachedToObjectId => _attachedToObjectId;
    public Vector3 AttachOffset => _attachOffset;
    public Vector3 AttachRotation => _attachRotation;
    public byte AttachmentSyncFlag => _attachmentSyncFlag;
    public CMatrix TargetMatrix => _targetMatrix;
    public bool IsMoving => _moving != 0;
    public float MovementSpeed => _movementSpeed;
    public CObjectMaterial Material => _material;
    public bool HasCustomMaterial => _hasCustomMaterial != 0;

    public CEntity* GetEntityPointer()
    {
        return (CEntity*)((byte*)Unsafe.AsPointer(ref this) + 0x000);
    }

    public float GetDistance(in CMatrix matrix)
    {
        CMatrix copy = matrix;
        return _getDistance((CObject*)Unsafe.AsPointer(ref this), &copy);
    }

    public void Stop()
    {
        _stop((CObject*)Unsafe.AsPointer(ref this));
    }

    public void SetRotation(Vector3 rotation)
    {
        Vector3 copy = rotation;
        _setRotation((CObject*)Unsafe.AsPointer(ref this), &copy);
    }

    public void SetAttachedToVehicle(ushort vehicleId, Vector3 offset, Vector3 rotation)
    {
        Vector3 offsetCopy = offset;
        Vector3 rotationCopy = rotation;
        _setAttachedToVehicle((CObject*)Unsafe.AsPointer(ref this), vehicleId, &offsetCopy, &rotationCopy);
    }

    public void SetAttachedToObject(ushort objectId, Vector3 offset, Vector3 rotation, byte attachmentFlag)
    {
        Vector3 offsetCopy = offset;
        Vector3 rotationCopy = rotation;
        _setAttachedToObject((CObject*)Unsafe.AsPointer(ref this), objectId, &offsetCopy, &rotationCopy, attachmentFlag);
    }

    public void AttachToVehicle(CVehicle* vehicle)
    {
        _attachToVehicle((CObject*)Unsafe.AsPointer(ref this), vehicle);
    }

    public void AttachToObject(CObject* obj)
    {
        _attachToObject((CObject*)Unsafe.AsPointer(ref this), obj);
    }

    public void Rotate(Vector3 rotation)
    {
        _rotate((CObject*)Unsafe.AsPointer(ref this), rotation);
    }

    public bool AttachedToMovingEntity()
    {
        return _attachedToMovingEntity((CObject*)Unsafe.AsPointer(ref this)) != 0;
    }

    public void SetMaterial(int modelId, int materialIndex, string txdName, string textureName, uint color)
    {
        using AnsiString txdAnsi = AnsiString.Encode(txdName);
        using AnsiString textureAnsi = AnsiString.Encode(textureName);
        _setMaterial((CObject*)Unsafe.AsPointer(ref this), modelId, materialIndex, txdAnsi, textureAnsi, color);
    }

    public void SetMaterialText(int materialIndex, string text, byte materialSize, string fontName, byte fontSize, bool bold, uint fontColor, uint backgroundColor, byte align)
    {
        using AnsiString textAnsi = AnsiString.Encode(text);
        using AnsiString fontAnsi = AnsiString.Encode(fontName);
        _setMaterialText((CObject*)Unsafe.AsPointer(ref this), materialIndex, textAnsi, materialSize, fontAnsi, fontSize, bold ? (byte)1 : (byte)0, fontColor, backgroundColor, align);
    }

    public bool TryGetMaterialSize(int materialSize, out int width, out int height)
    {
        int* size = stackalloc int[2];
        size[0] = 0;
        size[1] = 0;
        bool result = _getMaterialSize((CObject*)Unsafe.AsPointer(ref this), materialSize, &size[0], &size[1]) != 0;
        width = size[0];
        height = size[1];
        return result;
    }

    public void Render()
    {
        _render((CObject*)Unsafe.AsPointer(ref this));
    }

    public void Process(float elapsedTime)
    {
        _process((CObject*)Unsafe.AsPointer(ref this), elapsedTime);
    }

    public void ConstructMaterialText()
    {
        _constructMaterialText((CObject*)Unsafe.AsPointer(ref this));
    }

    public void Draw()
    {
        _draw((CObject*)Unsafe.AsPointer(ref this));
    }

    public void ShutdownMaterialText()
    {
        _shutdownMaterialText((CObject*)Unsafe.AsPointer(ref this));
    }

    public nint GetMaterialSpritePointer(int slot)
    {
        ValidateMaterialSlot(slot);
        return (nint)(*(int*)((byte*)Unsafe.AsPointer(ref this) + MaterialBaseOffset + MaterialSpritePointersOffset + (slot * sizeof(int))));
    }

    public uint GetMaterialColor(int slot)
    {
        ValidateMaterialSlot(slot);
        return *(uint*)((byte*)Unsafe.AsPointer(ref this) + MaterialBaseOffset + MaterialColorsOffset + (slot * sizeof(uint)));
    }

    public int GetMaterialType(int slot)
    {
        ValidateMaterialSlot(slot);
        return *(int*)((byte*)Unsafe.AsPointer(ref this) + MaterialBaseOffset + MaterialTypesOffset + (slot * sizeof(int)));
    }

    public bool HasMaterialSlot(int slot)
    {
        return GetMaterialType(slot) != 0;
    }

    public bool IsMaterialTextTextureCreated(int slot)
    {
        ValidateMaterialSlot(slot);
        return *(int*)((byte*)Unsafe.AsPointer(ref this) + MaterialBaseOffset + MaterialTextCreatedOffset + (slot * sizeof(int))) != 0;
    }

    public CObjectMaterialTextInfo GetMaterialTextInfo(int slot)
    {
        ValidateMaterialSlot(slot);
        return *(CObjectMaterialTextInfo*)((byte*)Unsafe.AsPointer(ref this) + MaterialBaseOffset + MaterialTextInfoOffset + (slot * MaterialTextInfoSize));
    }

    public nint GetMaterialTextPointer(int slot)
    {
        ValidateMaterialSlot(slot);
        return (nint)(*(int*)((byte*)Unsafe.AsPointer(ref this) + MaterialBaseOffset + MaterialTextPointersOffset + (slot * sizeof(int))));
    }

    public nint GetMaterialBackgroundTexturePointer(int slot)
    {
        ValidateMaterialSlot(slot);
        return (nint)(*(int*)((byte*)Unsafe.AsPointer(ref this) + MaterialBaseOffset + MaterialBackgroundTexturePointersOffset + (slot * sizeof(int))));
    }

    public nint GetMaterialTexturePointer(int slot)
    {
        ValidateMaterialSlot(slot);
        return (nint)(*(int*)((byte*)Unsafe.AsPointer(ref this) + MaterialBaseOffset + MaterialTexturePointersOffset + (slot * sizeof(int))));
    }

    private static void ValidateMaterialSlot(int slot)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(slot);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(slot, MaterialSlotCount);
    }
}

[StructLayout(LayoutKind.Explicit, Size = 3956, Pack = 1)]
public unsafe struct CObjectMaterial
{
    [FieldOffset(0x000)]
    public fixed int SpritePointers[16];

    [FieldOffset(0x040)]
    public fixed uint Colors[16];

    [FieldOffset(0x080)]
    public fixed byte Unknown0[68];

    [FieldOffset(0x0C4)]
    public fixed int Types[16];

    [FieldOffset(0x104)]
    public fixed int TextureWasCreated[16];

    [FieldOffset(0x145)]
    public fixed byte TextInfo[3440];

    [FieldOffset(0xEB4)]
    public fixed int TextPointers[16];

    [FieldOffset(0xEF4)]
    public fixed int BackgroundTexturePointers[16];

    [FieldOffset(0xF34)]
    public fixed int TexturePointers[16];
}

[StructLayout(LayoutKind.Explicit, Size = 215, Pack = 1)]
public unsafe struct CObjectMaterialTextInfo
{
    [FieldOffset(0x000)]
    public byte MaterialIndex;

    [FieldOffset(0x08A)]
    public byte MaterialSize;

    [FieldOffset(0x08B)]
    public fixed byte Font[65];

    [FieldOffset(0x0CC)]
    public byte FontSize;

    [FieldOffset(0x0CD)]
    public byte Bold;

    [FieldOffset(0x0CE)]
    public uint FontColor;

    [FieldOffset(0x0D2)]
    public uint BackgroundColor;

    [FieldOffset(0x0D6)]
    public byte Align;

    public string? GetFontName()
    {
        fixed (byte* font = Font)
        {
            return NativeString.Decode(font, 65);
        }
    }
}
