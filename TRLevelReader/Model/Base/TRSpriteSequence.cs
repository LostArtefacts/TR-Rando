using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRSpriteSequence : ISerializableCompact
    {
        public int SpriteID { get; set; }

        public short NegativeLength { get; set; }

        public short Offset { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" SpriteID: " + SpriteID);
            sb.Append(" NegativeLength: " + NegativeLength);
            sb.Append(" Offset: " + Offset);

            return sb.ToString();
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(SpriteID);
                    writer.Write(NegativeLength);
                    writer.Write(Offset);
                }

                return stream.ToArray();
            }
        }
    }
}
