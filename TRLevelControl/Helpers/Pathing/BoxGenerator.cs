using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using TRLevelControl.Model;

namespace TRLevelControl.Helpers.Pathing;

public class BoxGenerator
{
    public void Generate(TRRoom room, TR1Level level, TRRoomSector linkedSector)
    {
        Room boxRoom = Room.Create(room);
        Generate(boxRoom, (int)level.NumBoxes);

        List<TRBox> boxes = level.Boxes.ToList();
        foreach (Box box in boxRoom.Boxes)
        {
            TRBox trBox = box.ToTRBox(room.Info);
            boxes.Add(trBox);
            level.Boxes = boxes.ToArray();
            level.NumBoxes++;
            TR1BoxUtilities.UpdateOverlaps(level, trBox, box.Overlaps.Select(o => (ushort)o.Index).ToList());
            TR1BoxUtilities.DuplicateZone(level, linkedSector.BoxIndex);
        }
    }

    public void Generate(TR2Room room, TR2Level level, TRRoomSector linkedSector)
    {
        Room boxRoom = Room.Create(room);
        Generate(boxRoom, (int)level.NumBoxes);

        List<TR2Box> boxes = level.Boxes.ToList();
        foreach (Box box in boxRoom.Boxes)
        {
            TR2Box trBox = box.ToTR2Box(room.Info);
            boxes.Add(trBox);
            level.Boxes = boxes.ToArray();
            level.NumBoxes++;
            TR2BoxUtilities.UpdateOverlaps(level, trBox, box.Overlaps.Select(o => (ushort)o.Index).ToList());
            TR2BoxUtilities.DuplicateZone(level, linkedSector.BoxIndex);
        }
    }

    public void Generate(TR3Room room, TR3Level level, TRRoomSector linkedSector)
    {
        Room boxRoom = Room.Create(room);
        Generate(boxRoom, (int)level.NumBoxes);

        List<TR2Box> boxes = level.Boxes.ToList();
        foreach (Box box in boxRoom.Boxes)
        {
            TR2Box trBox = box.ToTR2Box(room.Info);
            boxes.Add(trBox);
            level.Boxes = boxes.ToArray();
            level.NumBoxes++;
            TR2BoxUtilities.UpdateOverlaps(level, trBox, box.Overlaps.Select(o => (ushort)o.Index).ToList());
            TR2BoxUtilities.DuplicateZone(level, (linkedSector.BoxIndex & 0x7FF0) >> 4);
        }

        ushort linkedMaterial = (ushort)(linkedSector.BoxIndex & 0x000F); // TR3-5 store material in bits 0-3 - wood, mud etc
        foreach (TRRoomSector sector in boxRoom.Sectors)
        {
            if (!sector.IsImpenetrable)
            {
                sector.BoxIndex <<= 4;
                sector.BoxIndex |= linkedMaterial;
            }
        }
    }

    private void Generate(Room room, int boxIndexStart)
    {
        for (sbyte height = room.Floor; height > room.Ceiling; height--)
        {
            FloorPlan plan = new FloorPlan(room.NumZSectors);

            for (int x = 1; x < room.NumXSectors - 1; x++)
            {
                for (int z = 1; z < room.NumZSectors - 1; z++)
                {
                    TRRoomSector sector = room.Sectors[x * room.NumZSectors + z];
                    if (sector.Floor == height)
                    {
                        plan.AddFloor(x, z);
                    }
                }
            }

            plan.GenerateBoxes(boxIndexStart, height);
            if (plan.Boxes.Count == 0)
            {
                continue;
            }

            room.FloorPlan.Add(plan);                
            boxIndexStart += plan.Boxes.Count;
            room.Boxes.AddRange(plan.Boxes);

            for (int x = 1; x < room.NumXSectors - 1; x++)
            {
                for (int z = 1; z < room.NumZSectors - 1; z++)
                {
                    TRRoomSector sector = room.Sectors[x * room.NumZSectors + z];
                    if (plan.GetBox(x, z) is Box box)
                    {
                        sector.BoxIndex = (ushort)box.Index;
                    }
                }
            }
        }

        for (int i = 0; i < room.Boxes.Count; i++)
        {
            Box box = room.Boxes[i];
            box.Overlaps = new List<Box>();

            for (int j = 0; j < room.Boxes.Count; j++)
            {
                Box neighbour = room.Boxes[j];
                if (i == j || box.Overlaps.Contains(neighbour))
                {
                    continue;
                }

                // Tiles may be touching at only one corner so we have to check if at least 2
                // points are shared.
                IEnumerable<Point> intersections = box.Points.Intersect(neighbour.Points);
                if (intersections.Count() > 1)
                {
                    box.Overlaps.Add(neighbour);
                }
            }
        }
    }
}
