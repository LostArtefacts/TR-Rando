using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR4Level : ISerializableCompact
    {
        public uint Version { get; set; }

        public ushort NumRoomTextiles { get; set; }

        public ushort NumObjTextiles { get; set; }

        public ushort NumBumpTextiles { get; set; }

        public TR4Texture32Chunk Texture32Chunk { get; set; }

        public TR4Texture16Chunk Texture16Chunk { get; set; }

        public TR4SkyAndFont32Chunk SkyAndFont32Chunk { get; set; }

        public TR4LevelDataChunk LevelDataChunk { get; set; }

        public uint NumSamples { get; set; }

        public TR4Sample[] Samples { get; set; }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
