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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" Atlas: " + Atlas);
            sb.Append(" X: " + X);
            sb.Append(" Y: " + Y);
            sb.Append(" Width: " + Width);
            sb.Append(" Height: " + Height);
            sb.Append(" LeftSide: " + LeftSide);
            sb.Append(" TopSide: " + TopSide);
            sb.Append(" RightSide: " + RightSide);
            sb.Append(" BottomSide: " + BottomSide);

            return sb.ToString();
        }
    }
}
