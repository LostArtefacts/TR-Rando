using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRZone : ISerializableCompact
    {
        public ushort GroundZone1Normal { get; set; }

        public ushort GroundZone2Normal { get; set; }

        public ushort FlyZoneNormal { get; set; }

        public ushort GroundZone1Alternate { get; set; }

        public ushort GroundZone2Alternate { get; set; }

        public ushort FlyZoneAlternate { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(GroundZone1Normal);
                    writer.Write(GroundZone2Normal);
                    writer.Write(FlyZoneNormal);
                    writer.Write(GroundZone1Alternate);
                    writer.Write(GroundZone2Alternate);
                    writer.Write(FlyZoneAlternate);
                }

                return stream.ToArray();
            }
        }
    }
}
