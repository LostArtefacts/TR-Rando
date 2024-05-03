using System.Drawing;
using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public class Box
{
    public int Index { get; set; }
    public int MinX { get; set; }
    public int MaxX { get; set; }
    public int MinZ { get; set; }
    public int MaxZ { get; set; }
    public sbyte Floor { get; set; }

    public List<Box> Overlaps { get; set; }

    private List<Point> _points;
    public List<Point> Points
    {
        get
        {
            if (_points == null)
            {
                _points = new List<Point>();
                for (int x = MinX; x <= MaxX; x++)
                {
                    for (int z = MinZ; z <= MaxZ; z++)
                    {
                        _points.Add(new Point(x, z));
                    }
                }
            }
            return _points;
        }
    }

    public TRBox ToTRBox(TRRoomInfo roomInfo)
    {
        uint xmin = (uint)(roomInfo.X + MinX * TRConsts.Step4);
        uint zmin = (uint)(roomInfo.Z + MinZ * TRConsts.Step4);
        uint xmax = (uint)(xmin + (MaxX - MinX) * TRConsts.Step4);
        uint zmax = (uint)(zmin + (MaxZ - MinZ) * TRConsts.Step4);
        return new()
        {
            XMin = xmin,
            ZMin = zmin,
            XMax = xmax,
            ZMax = zmax,
            TrueFloor = (short)(Floor * TRConsts.Step1)
        };
    }

    public override string ToString()
    {
        return string.Format("{0}: [{1}, {2}, {3}, {4}]", Index, MinX, MaxX, MinZ, MaxZ);
    }
}
