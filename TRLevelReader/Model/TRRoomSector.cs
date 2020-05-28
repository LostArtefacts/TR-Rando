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
    }
}
