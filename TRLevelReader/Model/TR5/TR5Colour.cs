using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR5Colour : ISerializableCompact
    {
        public float Red { get; set; }

        public float Green { get; set; }

        public float Blue { get; set; }

        public float Unused { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Red);
                    writer.Write(Green);
                    writer.Write(Blue);
                    writer.Write(Unused);
                }

                return stream.ToArray();
            }
        }
    }
}
