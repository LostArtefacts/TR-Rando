﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    // 4 bytes
    public class TRRoomSprite : ISerializableCompact
    {
        public short Vertex { get; set; }

        public short Texture { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" Vertex: " + Vertex);
            sb.Append(" Texture: " + Texture);

            return sb.ToString();
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Vertex);
                    writer.Write(Texture);
                }

                return stream.ToArray();
            }
        }
    }
}
