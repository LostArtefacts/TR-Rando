using System;
using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;
using TRLevelReader.Model.Base.Enums;

namespace TRLevelReader.Helpers
{
    public static class TR1BoxUtilities
    {
        public static void DuplicateZone(TRLevel level, int boxIndex)
        {
            TRZoneGroup zoneGroup = level.Zones[boxIndex];
            List<TRZoneGroup> zones = level.Zones.ToList();
            zones.Add(new TRZoneGroup
            {
                NormalZone = zoneGroup.NormalZone.Clone(),
                AlternateZone = zoneGroup.AlternateZone.Clone()
            });
            level.Zones = zones.ToArray();
        }

        public static TRZoneGroup[] ReadZones(uint numBoxes, ushort[] zoneData)
        {
            // Initialise the zone groups - one for every box.
            TRZoneGroup[] zones = new TRZoneGroup[numBoxes];
            for (int i = 0; i < zones.Length; i++)
            {
                zones[i] = new TRZoneGroup
                {
                    NormalZone = new TRZone(),
                    AlternateZone = new TRZone()
                };
            }

            // Build the zones, mapping the multidimensional ushort structures into the corresponding
            // zone object values.
            IEnumerable<FlipStatus> flipValues = Enum.GetValues(typeof(FlipStatus)).Cast<FlipStatus>();
            IEnumerable<TRZones> zoneValues = Enum.GetValues(typeof(TRZones)).Cast<TRZones>();

            int valueIndex = 0;
            foreach (FlipStatus flip in flipValues)
            {
                foreach (TRZones zone in zoneValues)
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

        public static ushort[] FlattenZones(TRZoneGroup[] zoneGroups)
        {
            // Convert the zone objects back into a flat ushort list.
            IEnumerable<FlipStatus> flipValues = Enum.GetValues(typeof(FlipStatus)).Cast<FlipStatus>();
            IEnumerable<TRZones> zoneValues = Enum.GetValues(typeof(TRZones)).Cast<TRZones>();

            List<ushort> zones = new List<ushort>();

            foreach (FlipStatus flip in flipValues)
            {
                foreach (TRZones zone in zoneValues)
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
    }
}