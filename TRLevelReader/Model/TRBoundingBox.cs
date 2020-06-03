using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRBoundingBox : ISerializableCompact
    {
        public short MinX { get; set; }

        public short MaxX { get; set; }

        public short MinY { get; set; }

        public short MaxY { get; set; }

        public short MinZ { get; set; }

        public short MaxZ { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" MinX: " + MinX);
            sb.Append(" MaxX: " + MaxX);
            sb.Append(" MinY: " + MinY);
            sb.Append(" MaxY: " + MaxY);
            sb.Append(" MinZ: " + MinZ);
            sb.Append(" MaxZ: " + MaxZ);

            return sb.ToString();
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(MinX);
                    writer.Write(MaxX);
                    writer.Write(MinY);
                    writer.Write(MaxY);
                    writer.Write(MinZ);
                    writer.Write(MaxZ);
                }

                return stream.ToArray();
            }
        }
    }
}
