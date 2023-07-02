﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRObjectTextureVert : ISerializableCompact
    {
        //Both are ufixed16 - 1 byte whole 1 byte fractional
        public FixedFloat16 XCoordinate { get; set; }

        public FixedFloat16 YCoordinate { get; set; }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(XCoordinate.Serialize());
                    writer.Write(YCoordinate.Serialize());
                }

                return stream.ToArray();
            }
        }
    }
}
