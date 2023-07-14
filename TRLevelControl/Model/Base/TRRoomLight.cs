﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRRoomLight : ISerializableCompact
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public ushort Intensity { get; set; }

        public uint Fade { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(X);
                    writer.Write(Y);
                    writer.Write(Z);
                    writer.Write(Intensity);
                    writer.Write(Fade);
                }

                return stream.ToArray();
            }
        }
    }
}
