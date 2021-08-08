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
    public class TR4Sample : ISerializableCompact
    {
        public uint UncompSize { get; set; }

        public uint CompSize { get; set; }

        public byte[] SoundData { get; set; }

        //Optional - mainly just for testing, this is just to store the raw zlib compressed chunk.
        public byte[] CompressedChunk { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(SoundData);
                }

                byte[] uncompressed = stream.ToArray();
                this.UncompSize = (uint)uncompressed.Length;

                byte[] compressed = TRZlib.Compress(uncompressed);
                this.CompSize = (uint)compressed.Length;

                return compressed;
            }
        }
    }
}
