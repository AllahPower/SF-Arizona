using System.Numerics;

namespace SFSharp;

public sealed unsafe class SFObject : SFEntity
{
    private readonly CObject* _native;

    internal SFObject(ushort id, CObject* native)
    {
        Id = id;
        _native = native;
    }

    public ushort Id { get; }
    public CObject* Native => _native;
    public new bool Exists => _native != null && CObjectPool.Instance.IsAllocated(Id);
    public int Model => _native is null ? 0 : _native->Model;
    public bool DontCollideWithCamera => _native is not null && _native->DontCollideWithCamera;
    public float DrawDistance => _native is null ? 0f : _native->DrawDistance;
    public float DistanceToCamera => _native is null ? 0f : _native->GetEntityPointer()->GetDistanceToCamera();
    public bool DrawLast => _native is not null && _native->DrawLast;
    public Vector3 Rotation => _native is null ? Vector3.Zero : _native->Rotation;
    public ushort AttachedToVehicleId => _native is null ? ushort.MaxValue : _native->AttachedToVehicleId;
    public ushort AttachedToObjectId => _native is null ? ushort.MaxValue : _native->AttachedToObjectId;
    public Vector3 AttachOffset => _native is null ? Vector3.Zero : _native->AttachOffset;
    public Vector3 AttachRotation => _native is null ? Vector3.Zero : _native->AttachRotation;
    public bool IsMoving => _native is not null && _native->IsMoving;
    public float MovementSpeed => _native is null ? 0f : _native->MovementSpeed;
    public bool HasCustomMaterial => _native is not null && _native->HasCustomMaterial;

    protected override CEntity* NativeEntity => _native is null ? null : _native->GetEntityPointer();

    public void Stop()
    {
        if (_native is not null)
        {
            _native->Stop();
        }
    }

    public void SetRotation(Vector3 rotation)
    {
        if (_native is not null)
        {
            _native->SetRotation(rotation);
        }
    }

    public void SetAttachedToVehicle(ushort vehicleId, Vector3 offset, Vector3 rotation)
    {
        if (_native is not null)
        {
            _native->SetAttachedToVehicle(vehicleId, offset, rotation);
        }
    }

    public void SetAttachedToObject(ushort objectId, Vector3 offset, Vector3 rotation, byte attachmentFlag)
    {
        if (_native is not null)
        {
            _native->SetAttachedToObject(objectId, offset, rotation, attachmentFlag);
        }
    }

    public void Rotate(Vector3 rotation)
    {
        if (_native is not null)
        {
            _native->Rotate(rotation);
        }
    }

    public bool IsAttachedToMovingEntity()
    {
        return _native is not null && _native->AttachedToMovingEntity();
    }

    public float GetDistance(in CMatrix matrix)
    {
        return _native is null ? 0f : _native->GetDistance(matrix);
    }

    public void SetMaterial(int modelId, int materialIndex, string txdName, string textureName, uint color)
    {
        if (_native is not null)
        {
            _native->SetMaterial(modelId, materialIndex, txdName, textureName, color);
        }
    }

    public void SetMaterialText(int materialIndex, string text, byte materialSize, string fontName, byte fontSize, bool bold, uint fontColor, uint backgroundColor, byte align)
    {
        if (_native is not null)
        {
            _native->SetMaterialText(materialIndex, text, materialSize, fontName, fontSize, bold, fontColor, backgroundColor, align);
        }
    }

    public bool TryGetMaterialSize(int materialSize, out int width, out int height)
    {
        if (_native is null)
        {
            width = 0;
            height = 0;
            return false;
        }

        return _native->TryGetMaterialSize(materialSize, out width, out height);
    }

    public void Render()
    {
        if (_native is not null)
        {
            _native->Render();
        }
    }

    public void Process(float elapsedTime)
    {
        if (_native is not null)
        {
            _native->Process(elapsedTime);
        }
    }

    public void ConstructMaterialText()
    {
        if (_native is not null)
        {
            _native->ConstructMaterialText();
        }
    }

    public void Draw()
    {
        if (_native is not null)
        {
            _native->Draw();
        }
    }

    public void ShutdownMaterialText()
    {
        if (_native is not null)
        {
            _native->ShutdownMaterialText();
        }
    }

    public SFObjectMaterialSlot GetMaterialSlot(int slot)
    {
        return new(this, slot);
    }

    public bool TryGetMaterialSlot(int slot, out SFObjectMaterialSlot materialSlot)
    {
        if (_native is null || slot < 0 || slot >= CObject.MaterialSlotCount)
        {
            materialSlot = default;
            return false;
        }

        materialSlot = new SFObjectMaterialSlot(this, slot);
        return true;
    }

    public IReadOnlyList<SFObjectMaterialSlot> EnumerateMaterialSlots(bool includeEmpty = false)
    {
        if (_native is null)
        {
            return [];
        }

        List<SFObjectMaterialSlot> slots = [];
        for (int slot = 0; slot < CObject.MaterialSlotCount; slot++)
        {
            SFObjectMaterialSlot materialSlot = new(this, slot);
            if (includeEmpty || materialSlot.Type != 0)
            {
                slots.Add(materialSlot);
            }
        }

        return slots;
    }
}
