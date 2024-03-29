﻿using System.Text;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRRoomSector : ISerializableCompact
{
    public ushort FDIndex { get; set; }

    public ushort BoxIndex { get; set; }

    public byte RoomBelow { get; set; }

    public sbyte Floor { get; set; }

    public byte RoomAbove { get; set; }

    public sbyte Ceiling { get; set; }

    public bool IsWall => Floor == TRConsts.WallClicks && Ceiling == TRConsts.WallClicks;
    public bool IsSlipperySlope => (BoxIndex & 0x7FF0) >> 4 == 2047;

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" FDIndex: " + FDIndex);
        sb.Append(" BoxIndex: " + BoxIndex);
        sb.Append(" RoomBelow: " + RoomBelow);
        sb.Append(" Floor: " + Floor);
        sb.Append(" RoomAbove: " + RoomAbove);
        sb.Append(" Ceiling: " + Ceiling);

        return sb.ToString();
    }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
            writer.Write(FDIndex);
            writer.Write(BoxIndex);
            writer.Write(RoomBelow);
            writer.Write(Floor);
            writer.Write(RoomAbove);
            writer.Write(Ceiling);
        }

        return stream.ToArray();
    }
}
