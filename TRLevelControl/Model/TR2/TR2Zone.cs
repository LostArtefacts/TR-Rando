using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRLevelControl.Model.TR2.Enums;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR2Zone : ISerializableCompact, ICloneable
{
    public Dictionary<TR2Zones, ushort> GroundZones { get; set; }
    public ushort FlyZone { get; set; }

    public TR2Zone()
    {
        GroundZones = new Dictionary<TR2Zones, ushort>();
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

    public TR2Zone Clone()
    {
        return new TR2Zone
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
