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

    public ExtRoomInfo(TRRoom room)
    {
        MinX = room.Info.X + TRConsts.Step4;
        MaxX = room.Info.X + TRConsts.Step4 * (room.NumXSectors - 1);
        MinZ = room.Info.Z + TRConsts.Step4;
        MaxZ = room.Info.Z + TRConsts.Step4 * (room.NumZSectors - 1);
        MinY = room.Info.YTop;
        MaxY = room.Info.YBottom;

        Width = room.NumXSectors - 2;
        Depth = room.NumZSectors - 2;
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
