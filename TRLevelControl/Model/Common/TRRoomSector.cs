namespace TRLevelControl.Model;

public class TRRoomSector
{
    public TRMaterial Material { get; set; }
    public ushort FDIndex { get; set; }
    public ushort BoxIndex { get; set; }
    public byte RoomBelow { get; set; }
    public sbyte Floor { get; set; }
    public byte RoomAbove { get; set; }
    public sbyte Ceiling { get; set; }

    public bool IsWall => Floor == TRConsts.WallClicks && Ceiling == TRConsts.WallClicks;
    public bool IsSlipperySlope => BoxIndex == 2047;
}
