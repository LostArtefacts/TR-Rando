using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model
{
    public class TR4TexImage32 : ISerializableCompact
    {
        public uint[] Tile { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (uint t in Tile) { writer.Write(t); }
                }

                return stream.ToArray();
            }
        }
    }
}
