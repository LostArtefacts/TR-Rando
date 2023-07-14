﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR2RoomVertex : ISerializableCompact
    {
        //6 bytes
        public TRVertex Vertex { get; set; }

        //2 bytes
        public short Lighting { get; set; }

        //2 bytes
        public ushort Attributes { get; set; }

        //2 bytes
        public short Lighting2 { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Vertex.Serialize());
                    writer.Write(Lighting);
                    writer.Write(Attributes);
                    writer.Write(Lighting2);
                }

                return stream.ToArray();
            }
        }
    }
}
