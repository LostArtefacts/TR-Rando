﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRBox : ISerializableCompact
    {
        public uint ZMin { get; set; }

        public uint ZMax { get; set; }

        public uint XMin { get; set; }

        public uint XMax { get; set; }

        public short TrueFloor { get; set; }

        public ushort OverlapIndex { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(ZMin);
                    writer.Write(ZMax);
                    writer.Write(XMin);
                    writer.Write(XMax);
                    writer.Write(TrueFloor);
                    writer.Write(OverlapIndex);
                }

                return stream.ToArray();
            }
        }
    }
}
