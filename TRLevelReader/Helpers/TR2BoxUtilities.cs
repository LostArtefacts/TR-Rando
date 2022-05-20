using System;
using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Base.Enums;
using TRLevelReader.Model.TR2.Enums;

namespace TRLevelReader.Helpers
{
    public class TR2BoxUtilities
    {
        public static readonly ushort BoxNumber = 0x7fff;
        public static readonly short OverlapIndex = 0x3fff;
        public static readonly ushort EndBit = 0x8000;
        public static readonly int Blockable = 0x8000;
        public static readonly int Blocked = 0x4000;

        public static readonly short TR2NoOverlap = -1;
        public static readonly short TR3NoOverlap = 2047;

        public static void DuplicateZone(TR2Level level, int boxIndex)
        {
            TR2ZoneGroup zoneGroup = level.Zones[boxIndex];
            List<TR2ZoneGroup> zones = level.Zones.ToList();
            zones.Add(new TR2ZoneGroup
            {
                NormalZone = zoneGroup.NormalZone.Clone(),
                AlternateZone = zoneGroup.AlternateZone.Clone()
            });
            level.Zones = zones.ToArray();
        }

        public static void DuplicateZone(TR3Level level, int boxIndex)
        {
            TR2ZoneGroup zoneGroup = level.Zones[boxIndex];
            List<TR2ZoneGroup> zones = level.Zones.ToList();
            zones.Add(new TR2ZoneGroup
            {
                NormalZone = zoneGroup.NormalZone.Clone(),
                AlternateZone = zoneGroup.AlternateZone.Clone()
            });
            level.Zones = zones.ToArray();
        }

        public static TR2ZoneGroup[] ReadZones(uint numBoxes, ushort[] zoneData)
        {
            // Initialise the zone groups - one for every box.
            TR2ZoneGroup[] zones = new TR2ZoneGroup[numBoxes];
            for (int i = 0; i < zones.Length; i++)
            {
                zones[i] = new TR2ZoneGroup
                {
                    NormalZone = new TR2Zone(),
                    AlternateZone = new TR2Zone()
                };
            }

            // Build the zones, mapping the multidimensional ushort structures into the corresponding
            // zone object values.
            IEnumerable<FlipStatus> flipValues = Enum.GetValues(typeof(FlipStatus)).Cast<FlipStatus>();
            IEnumerable<TR2Zones> zoneValues = Enum.GetValues(typeof(TR2Zones)).Cast<TR2Zones>();

            int valueIndex = 0;
            foreach (FlipStatus flip in flipValues)
            {
                foreach (TR2Zones zone in zoneValues)
                {
                    for (int box = 0; box < zones.Length; box++)
                    {
                        zones[box][flip].GroundZones[zone] = zoneData[valueIndex++];
                    }
                }

                for (int box = 0; box < zones.Length; box++)
                {
                    zones[box][flip].FlyZone = zoneData[valueIndex++];
                }
            }

            return zones;
        }

        public static ushort[] FlattenZones(TR2ZoneGroup[] zoneGroups)
        {
            // Convert the zone objects back into a flat ushort list.
            IEnumerable<FlipStatus> flipValues = Enum.GetValues(typeof(FlipStatus)).Cast<FlipStatus>();
            IEnumerable<TR2Zones> zoneValues = Enum.GetValues(typeof(TR2Zones)).Cast<TR2Zones>();

            List<ushort> zones = new List<ushort>();

            foreach (FlipStatus flip in flipValues)
            {
                foreach (TR2Zones zone in zoneValues)
                {
                    for (int box = 0; box < zoneGroups.Length; box++)
                    {
                        zones.Add(zoneGroups[box][flip].GroundZones[zone]);
                    }
                }

                for (int box = 0; box < zoneGroups.Length; box++)
                {
                    zones.Add(zoneGroups[box][flip].FlyZone);
                }
            }

            return zones.ToArray();
        }

        public static int GetSectorCount(TR2Level level, int boxIndex)
        {
            int count = 0;
            foreach (TR2Room room in level.Rooms)
            {
                foreach (TRRoomSector sector in room.SectorList)
                {
                    if (sector.BoxIndex == boxIndex)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static int GetSectorCount(TR3Level level, int boxIndex)
        {
            int count = 0;
            foreach (TR3Room room in level.Rooms)
            {
                foreach (TRRoomSector sector in room.Sectors)
                {
                    if ((sector.BoxIndex & 0x7FF0) >> 4 == boxIndex)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public static List<ushort> GetOverlaps(TR2Level level, TR2Box box)
        {
            return GetOverlaps(level.Overlaps, box, TR2NoOverlap);
        }

        public static List<ushort> GetOverlaps(TR3Level level, TR2Box box)
        {
            return GetOverlaps(level.Overlaps, box, TR3NoOverlap);
        }

        private static List<ushort> GetOverlaps(ushort[] allOverlaps, TR2Box box, short noOverlap)
        {
            List<ushort> overlaps = new List<ushort>();

            if (box.OverlapIndex != noOverlap)
            {
                int index = box.OverlapIndex & OverlapIndex;
                ushort boxNumber;
                bool done = false;
                do
                {
                    boxNumber = allOverlaps[index++];
                    if ((boxNumber & EndBit) > 0)
                    {
                        done = true;
                        boxNumber &= BoxNumber;
                    }
                    overlaps.Add(boxNumber);
                }
                while (!done);
            }

            return overlaps;
        }

        public static void UpdateOverlaps(TR2Level level, TR2Box box, List<ushort> overlaps)
        {
            List<ushort> newOverlaps = new List<ushort>();
            foreach (TR2Box lvlBox in level.Boxes)
            {
                // Either append the current overlaps, or the new ones if this is the box being updated.
                // Do nothing for boxes that have no overlaps.
                List<ushort> boxOverlaps = lvlBox == box ? overlaps : GetOverlaps(level, lvlBox);
                UpdateOverlaps(lvlBox, boxOverlaps, newOverlaps, TR2NoOverlap);
            }

            // Update the level data
            level.Overlaps = newOverlaps.ToArray();
            level.NumOverlaps = (uint)newOverlaps.Count;
        }

        public static void UpdateOverlaps(TR3Level level, TR2Box box, List<ushort> overlaps)
        {
            List<ushort> newOverlaps = new List<ushort>();
            foreach (TR2Box lvlBox in level.Boxes)
            {
                List<ushort> boxOverlaps = lvlBox == box ? overlaps : GetOverlaps(level, lvlBox);
                UpdateOverlaps(lvlBox, boxOverlaps, newOverlaps, TR3NoOverlap);
            }

            // Update the level data
            level.Overlaps = newOverlaps.ToArray();
            level.NumOverlaps = (uint)newOverlaps.Count;
        }

        private static void UpdateOverlaps(TR2Box lvlBox, List<ushort> boxOverlaps, List<ushort> newOverlaps, short noOverlap)
        {
            // Either append the current overlaps, or the new ones if this is the box being updated.
            // Do nothing for boxes that have no overlaps.
            if (boxOverlaps.Count > 0)
            {
                // TR2 uses -1 as NoOverlap, but TR3+ uses 2047. So we can never use 2047
                // as an index itself. Add a dummy entry, which will never be referenced.
                if (newOverlaps.Count == noOverlap)
                {
                    newOverlaps.Add(0);
                }

                // Mark the overlap offset for this box to the insertion point
                UpdateOverlapIndex(lvlBox, newOverlaps.Count);

                for (int i = 0; i < boxOverlaps.Count; i++)
                {
                    ushort index = boxOverlaps[i];
                    if (i == boxOverlaps.Count - 1)
                    {
                        // Make sure the final overlap has the end bit set.
                        index |= EndBit;
                    }
                    newOverlaps.Add(index);
                }
            }
        }

        private static void UpdateOverlapIndex(TR2Box box, int index)
        {
            if ((box.OverlapIndex & Blockable) > 0)
            {
                index |= Blockable;
            }
            if ((box.OverlapIndex & Blocked) > 0)
            {
                index |= Blocked;
            }
            box.OverlapIndex = (short)index;
        }
    }
}