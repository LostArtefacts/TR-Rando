using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model
{
    public class TR3RoomSpotlight : ISerializableCompact
    {
        public int Intensity { get; set; }

        public int Fade { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Intensity);
                    writer.Write(Fade);
                }

                return stream.ToArray();
            }
        }
    }
}
