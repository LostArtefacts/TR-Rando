using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR4MeshFace4 : ISerializableCompact
    {
        public ushort[] Vertices { get; set; }

        public ushort Texture { get; set; }

        public ushort Effects { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {

                }

                return stream.ToArray();
            }
        }
    }
}
