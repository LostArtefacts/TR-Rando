using TRLevelControl;
using TRLevelControl.Model;

namespace TRRandomizerCore.Helpers;

public class ExtRoomInfo
{
    public int MinX { get; private set; }
    public int MaxX { get; private set; }
    public int MinZ { get; private set; }
    public int MaxZ { get; private set; }
    public int MinY { get; private set; }
    public int MaxY { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Depth { get; private set; }

    public int Size { get; private set; }

    public ExtRoomInfo(TRRoomInfo info, int numXSectors, int numZSectors)
    {
        MinX = info.X + TRConsts.Step4;
        MaxX = info.X + TRConsts.Step4 * (numXSectors - 1);
        MinZ = info.Z + TRConsts.Step4;
        MaxZ = info.Z + TRConsts.Step4 * (numZSectors - 1);
        MinY = info.YTop;
        MaxY = info.YBottom;

        Width = numXSectors - 2;
        Depth = numZSectors - 2;
        Height = Math.Abs(MaxY - MinY) / TRConsts.Step1;

        Size = Width * Depth * Height;
    }

    public bool Contains(Location location)
    {
        return location.X >= MinX && location.X < MaxX
            && location.Y > MinY && location.Y <= MaxY
            && location.Z >= MinZ && location.Z < MaxZ;
    }
}
