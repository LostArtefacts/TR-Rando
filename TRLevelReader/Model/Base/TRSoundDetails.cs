using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRSoundDetails : ISerializableCompact
    {
        public ushort Sample { get; set; }

        public ushort Volume { get; set; }

        public ushort Chance { get; set; }

        public ushort Characteristics { get; set; }

        public int NumSounds
        {
            get
            {
                return (Characteristics & 0x00FC) >> 2; // get bits 2-7
            }
            set
            {
                Characteristics = (ushort)(Characteristics & ~(Characteristics & 0x00FC));
                Characteristics |= (ushort)(value << 2);
            }
        }

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
                    Characteristics = (ushort)(Characteristics & ~0x2000);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" Sample: " + Sample);
            sb.Append(" Volume: " + Volume);
            sb.Append(" Chance: " + Chance);
            sb.Append(" Characteristics: " + Characteristics.ToString("X4"));
            sb.Append(" NumSounds: " + NumSounds);

            return sb.ToString();
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Sample);
                    writer.Write(Volume);
                    writer.Write(Chance);
                    writer.Write(Characteristics);
                }

                return stream.ToArray();
            }
        }
    }
}
