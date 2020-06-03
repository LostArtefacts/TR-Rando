using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR2RoomStaticMesh : ISerializableCompact
    {
        public uint X { get; set; }

        public uint Y { get; set; }

        public uint Z { get; set; }

        public ushort Rotation { get; set; }

        public ushort Intensity1 { get; set; }

        public ushort Intensity2 { get; set; }

        public ushort MeshID { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" X: " + X);
            sb.Append(" Y: " + Y);
            sb.Append(" Z: " + Z);
            sb.Append(" Rotation: " + Rotation);
            sb.Append(" Int1: " + Intensity1);
            sb.Append(" Int2: " + Intensity2);
            sb.Append(" MeshID: " + MeshID);

            return sb.ToString();
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(X);
                    writer.Write(Y);
                    writer.Write(Z);
                    writer.Write(Rotation);
                    writer.Write(Intensity1);
                    writer.Write(Intensity2);
                    writer.Write(MeshID);
                }

                return stream.ToArray();
            }
        }
    }
}
