using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR2Entity : ISerializableCompact
    {
        public short TypeID { get; set; }

        public short Room { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public short Angle { get; set; }

        public short Intensity1 { get; set; }

        public short Intensity2 { get; set; }

        public ushort Flags { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" TypeID: " + TypeID);
            sb.Append(" Room: " + Room);
            sb.Append(" X: " + X);
            sb.Append(" Y: " + Y);
            sb.Append(" Z: " + Z);
            sb.Append(" Angle: " + Angle);
            sb.Append(" Int1: " + Intensity1);
            sb.Append(" Int2: " + Intensity2);
            sb.Append(" Flags " + Flags.ToString("X4"));

            return sb.ToString();
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(TypeID);
                    writer.Write(Room);
                    writer.Write(X);
                    writer.Write(Y);
                    writer.Write(Z);
                    writer.Write(Angle);
                    writer.Write(Intensity1);
                    writer.Write(Intensity2);
                    writer.Write(Flags);
                }

                return stream.ToArray();
            }
        }
    }
}
