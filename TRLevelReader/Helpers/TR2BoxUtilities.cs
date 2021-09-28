using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRLevelReader.Model;
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

        public static ushort GetZoneValue(TR2Level level, TR2Zones zone, FlipStatus flip, int boxIndex, Normal normal = Normal.Normal1)
        {
            return zone == TR2Zones.Ground ? level.GroundZone[(int)flip][(int)normal][boxIndex] : level.FlyZone[(int)flip][boxIndex];
        }

        public static TR2Zone GetZone(TR2Level level, int boxIndex)
        {
            // This is just a representation of the zoning, we use the manual approach instead
            return new TR2Zone
            {
                GroundZone1Normal = level.GroundZone[(int)FlipStatus.Off][(int)Normal.Normal1][boxIndex],
                GroundZone2Normal = level.GroundZone[(int)FlipStatus.Off][(int)Normal.Normal2][boxIndex],
                GroundZone3Normal = level.GroundZone[(int)FlipStatus.Off][(int)Normal.Normal3][boxIndex],
                GroundZone4Normal = level.GroundZone[(int)FlipStatus.Off][(int)Normal.Normal4][boxIndex],

                GroundZone1Alternate = level.GroundZone[(int)FlipStatus.On][(int)Normal.Normal1][boxIndex],
                GroundZone2Alternate = level.GroundZone[(int)FlipStatus.On][(int)Normal.Normal2][boxIndex],
                GroundZone3Alternate = level.GroundZone[(int)FlipStatus.On][(int)Normal.Normal3][boxIndex],
                GroundZone4Alternate = level.GroundZone[(int)FlipStatus.On][(int)Normal.Normal4][boxIndex],

                FlyZoneNormal = level.FlyZone[(int)FlipStatus.Off][boxIndex],
                FlyZoneAlternate = level.FlyZone[(int)FlipStatus.On][boxIndex]
            };
        }

        public static void DuplicateZone(TR2Level level, int boxIndex)
        {
            IEnumerable<int> flipValues = Enum.GetValues(typeof(FlipStatus)).Cast<int>();
            IEnumerable<int> normValues = Enum.GetValues(typeof(Normal)).Cast<int>();

            foreach (int flip in flipValues)
            {
                foreach (int norm in normValues)
                {
                    List<ushort> groundValues = level.GroundZone[flip][norm].ToList();
                    groundValues.Add(groundValues[boxIndex]);
                    level.GroundZone[flip][norm] = groundValues.ToArray();
                }

                List<ushort> flyValues = level.FlyZone[flip].ToList();
                flyValues.Add(flyValues[boxIndex]);
                level.FlyZone[flip] = flyValues.ToArray();
            }
        }

        public static void ReadZones(TR2Level level, BinaryReader reader)
        {
            IEnumerable<int> flipValues = Enum.GetValues(typeof(FlipStatus)).Cast<int>();
            IEnumerable<int> normValues = Enum.GetValues(typeof(Normal)).Cast<int>();

            level.GroundZone = new ushort[flipValues.Count()][][];
            level.FlyZone = new ushort[flipValues.Count()][];

            foreach (int flip in flipValues)
            {
                level.GroundZone[flip] = new ushort[normValues.Count()][];
                foreach (int norm in normValues)
                {
                    level.GroundZone[flip][norm] = new ushort[level.NumBoxes];
                    for (int box = 0; box < level.NumBoxes; box++)
                    {
                        level.GroundZone[flip][norm][box] = reader.ReadUInt16();
                    }
                }

                level.FlyZone[flip] = new ushort[level.NumBoxes];
                for (int box = 0; box < level.NumBoxes; box++)
                {
                    level.FlyZone[flip][box] = reader.ReadUInt16();
                }
            }
        }

        public static ushort[] FlattenZones(TR2Level level)
        {
            IEnumerable<FlipStatus> flipValues = Enum.GetValues(typeof(FlipStatus)).Cast<FlipStatus>();
            IEnumerable<Normal> normValues = Enum.GetValues(typeof(Normal)).Cast<Normal>();

            List<ushort> zones = new List<ushort>();

            foreach (FlipStatus flip in flipValues)
            {
                foreach (Normal norm in normValues)
                {
                    for (int box = 0; box < level.NumBoxes; box++)
                    {
                        zones.Add(GetZoneValue(level, TR2Zones.Ground, flip, box, norm));
                    }
                }

                for (int box = 0; box < level.NumBoxes; box++)
                {
                    zones.Add(GetZoneValue(level, TR2Zones.Fly, flip, box));
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

        public static List<ushort> GetOverlaps(TR2Level level, TR2Box box)
        {
            List<ushort> overlaps = new List<ushort>();

            if (box.OverlapIndex != -1)
            {
                int index = box.OverlapIndex & OverlapIndex;
                ushort boxNumber;
                bool done = false;
                do
                {
                    boxNumber = level.Overlaps[index++];
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

        public static int InsertOverlaps(TR2Level level, List<ushort> overlaps)
        {
            // This is inefficient currently as we just append rather than shifting
            // everything else, so previous entries remain.
            List<ushort> levelOverlaps = level.Overlaps.ToList();
            for (int i = 0; i < overlaps.Count; i++)
            {
                ushort index = overlaps[i];
                if (i == overlaps.Count - 1)
                {
                    index |= EndBit;
                }
                levelOverlaps.Add(index);
            }

            int newIndex = (int)level.NumOverlaps;
            level.Overlaps = levelOverlaps.ToArray();
            level.NumOverlaps = (uint)levelOverlaps.Count;

            return newIndex;
        }

        public static void UpdateOverlapIndex(TR2Box box, int index)
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

        public static void UpdateOverlaps(TR2Level level, TR2Box box, List<ushort> overlaps)
        {
            int index = InsertOverlaps(level, overlaps);
            UpdateOverlapIndex(box, index);
        }
    }
}