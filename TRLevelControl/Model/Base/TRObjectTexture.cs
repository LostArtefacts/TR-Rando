using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model
{
    public class TRObjectTexture : ISerializableCompact
    {
        public ushort Attribute { get; set; }

        public ushort AtlasAndFlag { get; set; }

        public TRObjectTextureVert[] Vertices { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Attribute);
                    writer.Write(AtlasAndFlag);

                    foreach (TRObjectTextureVert vert in Vertices)
                    {
                        writer.Write(vert.Serialize());
                    }
                }

                return stream.ToArray();
            }
        }
    }
}
