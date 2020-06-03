using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TRRoomSector : ISerializableCompact
    {
        public ushort FDIndex { get; set; }

        public ushort BoxIndex { get; set; }

        public byte RoomBelow { get; set; }

        public sbyte Floor { get; set; }

        public byte RoomAbove { get; set; }

        public sbyte Ceiling { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" FDIndex: " + FDIndex);
            sb.Append(" BoxIndex: " + BoxIndex);
            sb.Append(" RoomBelow: " + RoomBelow);
            sb.Append(" Floor: " + Floor);
            sb.Append(" RoomAbove: " + RoomAbove);
            sb.Append(" Ceiling: " + Ceiling);

            return sb.ToString();
        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(FDIndex);
                    writer.Write(BoxIndex);
                    writer.Write(RoomBelow);
                    writer.Write(Floor);
                    writer.Write(RoomAbove);
                    writer.Write(Ceiling);
                }

                return stream.ToArray();
            }
        }
    }
}
