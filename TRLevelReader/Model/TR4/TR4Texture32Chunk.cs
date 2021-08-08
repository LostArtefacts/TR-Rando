using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Compression;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR4Texture32Chunk : ISerializableCompact
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
                    foreach (TR4TexImage32 tex in Textiles) { writer.Write(tex.Serialize()); }
                }

                byte[] uncompressed = stream.ToArray();
                this.UncompressedSize = (uint)uncompressed.Length;

                byte[] compressed = TRZlib.Compress(uncompressed);
                this.CompressedSize = (uint)compressed.Length;

                return compressed;
            }
        }
    }
}
