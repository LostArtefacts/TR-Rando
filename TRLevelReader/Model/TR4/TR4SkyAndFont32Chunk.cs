using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR4SkyAndFont32Chunk : ISerializableCompact
    {
        public uint UncompressedSize { get; set; }

        public uint CompressedSize { get; set; }

        public TR4TexImage32[] Textiles { get; set; }

        //Optional - mainly just for testing, this is just to store the raw zlib compressed chunk.
        public byte[] CompressedChunk { get; set; }

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
