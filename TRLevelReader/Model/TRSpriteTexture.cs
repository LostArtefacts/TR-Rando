using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRSpriteTexture
    {
        public ushort Atlas { get; set; }

        public byte X { get; set; }

        public byte Y { get; set; }

        public ushort Width { get; set; }

        public ushort Height { get; set; }

        public short LeftSide { get; set; }

        public short TopSide { get; set; }

        public short RightSide { get; set; }

        public short BottomSide { get; set; }
    }
}
