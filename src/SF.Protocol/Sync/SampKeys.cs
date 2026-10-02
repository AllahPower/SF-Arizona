namespace SFSharp.Protocol.Sync;

public readonly record struct SampKeys(
    bool PrimaryFire, bool HornCrouch, bool SecondaryFire, bool AccelZoomOut,
    bool EnterExitCar, bool DecelJump, bool CircleRight, bool Aim,
    bool CircleLeft, bool LandingGearLookback, bool WalkSlow,
    bool SpecialCtrlUp, bool SpecialCtrlDown, bool SpecialCtrlLeft, bool SpecialCtrlRight)
{
    public static SampKeys Parse(ushort raw)
    {
        return new SampKeys(
            PrimaryFire: (raw & (1 << 0)) != 0,
            HornCrouch: (raw & (1 << 1)) != 0,
            SecondaryFire: (raw & (1 << 2)) != 0,
            AccelZoomOut: (raw & (1 << 3)) != 0,
            EnterExitCar: (raw & (1 << 4)) != 0,
            DecelJump: (raw & (1 << 5)) != 0,
            CircleRight: (raw & (1 << 6)) != 0,
            Aim: (raw & (1 << 7)) != 0,
            CircleLeft: (raw & (1 << 8)) != 0,
            LandingGearLookback: (raw & (1 << 9)) != 0,
            WalkSlow: (raw & (1 << 10)) != 0,
            SpecialCtrlUp: (raw & (1 << 11)) != 0,
            SpecialCtrlDown: (raw & (1 << 12)) != 0,
            SpecialCtrlLeft: (raw & (1 << 13)) != 0,
            SpecialCtrlRight: (raw & (1 << 14)) != 0);
    }
}

public readonly record struct SampAnimation(ushort Id, byte FrameDelta, byte Flags)
{
    public bool LoopA => (Flags & 1) != 0;
    public bool LockX => (Flags & 2) != 0;
    public bool LockY => (Flags & 4) != 0;
    public bool LockF => (Flags & 8) != 0;
    public byte Time => (byte)(Flags >> 4);
    public uint RawValue => (uint)(Id | (FrameDelta << 16) | (Flags << 24));

    public static SampAnimation FromRaw(uint rawValue)
    {
        return new SampAnimation((ushort)rawValue, (byte)(rawValue >> 16), (byte)(rawValue >> 24));
    }
}
