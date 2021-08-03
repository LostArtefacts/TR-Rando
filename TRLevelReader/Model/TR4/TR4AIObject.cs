using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
    public class TR4AIObject : ISerializableCompact
    {
        public ushort TypeID { get; set; }

        public ushort Room { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public short OCB { get; set; }

        public ushort Flags { get; set; }

        public int Angle { get; set; }

        public byte[] Serialize()
        {
            throw new NotImplementedException();
        }
    }
}
