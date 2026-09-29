namespace SFSharp.Runtime.Interop.Offsets;

public static class GtaOffsets
{
    // GTA SA 1.0 US / current Arizona client. RVAs are relative to the EXE base,
    // unlike the legacy absolute camera addresses below. Device/HWND verified by
    // read-only process inspection on 2026-09-29; function identities checked in IDA.
    public static class RenderWareRva
    {
        public const int D3D9Device = 0x897C28;
        public const int WindowHandle = 0x897C1C;
        public const int RasterShowRaster = 0x3F99B0;
        public const int CameraBeginUpdate = 0x3F8F20;
        public const int CameraEndUpdate = 0x3F98D0;
    }

    public static class InputRva
    {
        public const int MainWndProc = 0x347EB0;
        public const int UpdateMouse = 0x13F3C0;
        public const int GetMouseState = 0x346ED0;
    }

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
