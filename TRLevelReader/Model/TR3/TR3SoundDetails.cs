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

        public int NumSounds => (Characteristics & 0x00FC) >> 2; // get bits 2-7

        public bool Wibble
        {
            get
            {
                return (Characteristics & 0x2000) > 0;
            }
            set
            {
                if (value)
                {
                    Characteristics |= 0x2000;
                }
                else
                {
                    Characteristics = (short)(Characteristics & ~0x2000);
                }
            }
        }

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
