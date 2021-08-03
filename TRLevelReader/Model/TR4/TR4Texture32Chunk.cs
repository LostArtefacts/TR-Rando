using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR4Texture32Chunk : ISerializableCompact
    {
        public uint UncompressedSize { get; set; }

        public uint CompressedSize { get; set; }

        public TR4TexImage32[] Textiles { get; set; }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
