using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model
{
    public class TRColour : ISerializableCompact
    {
        public byte Red { get; set; }

        public byte Green { get; set; }
        
        public byte Blue { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" Red: " + Red);
            sb.Append(" Green: " + Green);
            sb.Append(" Blue: " + Blue);

            return sb.ToString();
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Red);
                    writer.Write(Green);
                    writer.Write(Blue);
                }

                return stream.ToArray();
            }
        }
    }
}
