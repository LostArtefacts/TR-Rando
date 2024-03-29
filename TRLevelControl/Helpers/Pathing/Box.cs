﻿using System.Drawing;
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
        // World coordinates and max points are 1 unit shy off edge
        uint xmin = (uint)(roomInfo.X + MinX * TRConsts.Step4);
        uint zmin = (uint)(roomInfo.Z + MinZ * TRConsts.Step4);
        uint xmax = (uint)(xmin + (MaxX - MinX) * TRConsts.Step4) - 1;
        uint zmax = (uint)(zmin + (MaxZ - MinZ) * TRConsts.Step4) - 1;
        return new TRBox
        {
            XMin = xmin,
            ZMin = zmin,
            XMax = xmax,
            ZMax = zmax,
            TrueFloor = (short)(Floor * TRConsts.Step1)
        };
    }

    public TR2Box ToTR2Box(TRRoomInfo roomInfo)
    {
        // Sector counts
        byte xmin = (byte)(roomInfo.X / TRConsts.Step4 + MinX);
        byte zmin = (byte)(roomInfo.Z / TRConsts.Step4 + MinZ);
        byte xmax = (byte)(xmin + (MaxX - MinX));
        byte zmax = (byte)(zmin + (MaxZ - MinZ));
        return new TR2Box
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
