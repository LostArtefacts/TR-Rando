namespace TRLevelControl;

public static class TRConsts
{
    public const int TPageWidth = 256;
    public const int TPageHeight = 256;
    public const int TPageSize = TPageWidth * TPageHeight;

    public const int PaletteSize = 256;
    public const int Palette8Multiplier = 4;
    public const int LightMapSize = PaletteSize * 32;

    public const int Step1 = 256;
    public const int Step2 = Step1 * 2;
    public const int Step3 = Step1 * 3;
    public const int Step4 = Step1 * 4;

    public const int NoRoom = 255;
    public const int WallShift = 10;
    public const int NoHeight = -32512;
    public const int WallClicks = -127;

    public const int MaskBits = 5;
    public const int FullMask = (1 << MaskBits) - 1;

    public const int Angle45 = 8192;
}
