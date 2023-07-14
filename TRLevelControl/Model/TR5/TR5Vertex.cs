using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model
{
    public class TR5Vertex : ISerializableCompact
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(X);
                    writer.Write(Y);
                    writer.Write(Z);
                }

                return stream.ToArray();
            }
        }
    }
}
