using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR4Sample : ISerializableCompact
    {
        public uint UncompSize { get; set; }

        public uint CompSize { get; set; }

        public byte[] SoundData { get; set; }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
