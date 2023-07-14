using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR3RoomStaticMesh : ISerializableCompact
    {
        public uint X { get; set; }

        public uint Y { get; set; }

        public uint Z { get; set; }

        public ushort Rotation { get; set; }

        public ushort Colour { get; set; }

        public ushort Unused { get; set; }

        public ushort MeshID { get; set; }

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
                    writer.Write(Colour);
                    writer.Write(Unused);
                    writer.Write(MeshID);
                }

                return stream.ToArray();
            }
        }
    }
}
