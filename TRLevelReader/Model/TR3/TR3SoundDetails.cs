using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR3SoundDetails : ISerializableCompact
    {
        public ushort Sample { get; set; }

        public byte Volume { get; set; }

        public byte Range { get; set; }

        public byte Chance { get; set; }

        public byte Pitch { get; set; }

        public short Characteristics { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Sample);
                    writer.Write(Volume);
                    writer.Write(Range);
                    writer.Write(Chance);
                    writer.Write(Pitch);
                    writer.Write(Characteristics);
                }

                return stream.ToArray();
            }
        }
    }
}
