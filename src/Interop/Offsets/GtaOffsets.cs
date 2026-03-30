namespace SFSharp;

public static class GtaOffsets
{
    public static class CCamera
    {
        public const nint TheCamera = 0xB6F028;
        public const int Size = 0xD78;

        public const int WideScreenOn = 0x70;
        public const int ShakeForce = 0x74;
        public const int FadeAlpha = 0x7C;
        public const int FadeState = 0x7E;

        public const int ActiveCam = 0x174;
    }

    public static class CCam
    {
        public const int Size = 0x238;
        public const int Mode = 0x00C;
        public const int Fov = 0x040;
        public const int Source = 0x0F0;
        public const int Front = 0x138;
        public const int Up = 0x168;
    }
}
