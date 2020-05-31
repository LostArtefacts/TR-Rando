using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRRoomSector
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
    }
}
