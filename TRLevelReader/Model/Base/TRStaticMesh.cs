using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRStaticMesh : ISerializableCompact
    {
        public uint ID { get; set; }

        public ushort Mesh { get; set; }

        public TRBoundingBox VisibilityBox { get; set; }

        public TRBoundingBox CollisionBox { get; set; }

        public ushort Flags { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(ID);
                    writer.Write(Mesh);
                    writer.Write(VisibilityBox.Serialize());
                    writer.Write(CollisionBox.Serialize());
                    writer.Write(Flags);
                }

                return stream.ToArray();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" ID: " + ID);
            sb.Append(" Mesh: " + Mesh);
            sb.Append(" VisibilityBox: {" + VisibilityBox.ToString() + "} ");
            sb.Append(" CollisionBox: {" + CollisionBox.ToString() + "} ");
            sb.Append(" Flags: " + Flags.ToString("X4"));

            return sb.ToString();
        }
    }
}
