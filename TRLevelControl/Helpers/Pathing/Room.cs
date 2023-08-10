using System.Text;
using TRLevelControl.Model;

namespace TRLevelControl.Helpers;

public class Room
{
    public sbyte Floor { get; set; }
    public sbyte Ceiling { get; set; }
    public int NumXSectors { get; set; }
    public int NumZSectors { get; set; }
    public List<TRRoomSector> Sectors { get; set; }
    public List<FloorPlan> FloorPlan { get; set; }
    public List<Box> Boxes { get; set; }

    public static Room Create(TRRoom room)
    {
        return new Room
        {
            Floor = (sbyte)(room.Info.YBottom / 256),
            Ceiling = (sbyte)(room.Info.YTop / 256),
            NumXSectors = room.NumXSectors,
            NumZSectors = room.NumZSectors,
            Sectors = room.Sectors.ToList(),
            FloorPlan = new List<FloorPlan>(),
            Boxes = new List<Box>()
        };
    }

    public static Room Create(TR2Room room)
    {
        return new Room
        {
            Floor = (sbyte)(room.Info.YBottom / 256),
            Ceiling = (sbyte)(room.Info.YTop / 256),
            NumXSectors = room.NumXSectors,
            NumZSectors = room.NumZSectors,
            Sectors = room.SectorList.ToList(),
            FloorPlan = new List<FloorPlan>(),
            Boxes = new List<Box>()
        };
    }

    public static Room Create(TR3Room room)
    {
        return new Room
        {
            Floor = (sbyte)(room.Info.YBottom / 256),
            Ceiling = (sbyte)(room.Info.YTop / 256),
            NumXSectors = room.NumXSectors,
            NumZSectors = room.NumZSectors,
            Sectors = room.Sectors.ToList(),
            FloorPlan = new List<FloorPlan>(),
            Boxes = new List<Box>()
        };
    }

    public override string ToString()
    {
        StringBuilder sb = new();
        for (int z = NumZSectors - 1; z >= 0; z--)
        {
            for (int x = 0; x < NumXSectors; x++)
            {
                TRRoomSector sector = Sectors[x * NumZSectors + z];
                sb.Append((sector.IsImpenetrable ? "WALL" : sector.BoxIndex.ToString().PadLeft(4, '0')) + " ");
            }
            sb.AppendLine();
        }
        foreach (Box box in Boxes)
        {
            sb.AppendLine(string.Format("{0}: {1}", box.Index.ToString().PadLeft(4, '0'), string.Join(",", box.Overlaps.Select(b => b.Index.ToString().PadLeft(4, '0')))));
        }
        sb.AppendLine();
        return sb.ToString();
    }
}
