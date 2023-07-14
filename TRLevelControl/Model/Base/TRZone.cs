using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRLevelReader.Model.Base.Enums;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRZone : ISerializableCompact, ICloneable
    {
        public Dictionary<TRZones, ushort> GroundZones { get; set; }
        public ushort FlyZone { get; set; }

        public TRZone()
        {
            GroundZones = new Dictionary<TRZones, ushort>();
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (ushort zone in GroundZones.Values)
                    {
                        writer.Write(zone);
                    }
                    writer.Write(FlyZone);
                }

                return stream.ToArray();
            }
        }

        public TRZone Clone()
        {
            return new TRZone
            {
                GroundZones = GroundZones.ToDictionary(e => e.Key, e => e.Value),
                FlyZone = FlyZone
            };
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}