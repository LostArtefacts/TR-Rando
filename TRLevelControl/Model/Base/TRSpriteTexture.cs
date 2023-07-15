using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Serialization;

namespace TRLevelControl.Model
{
    public class TRSpriteTexture : ISerializableCompact
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

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Atlas);
                    writer.Write(X);
                    writer.Write(Y);
                    writer.Write(Width);
                    writer.Write(Height);
                    writer.Write(LeftSide);
                    writer.Write(TopSide);
                    writer.Write(RightSide);
                    writer.Write(BottomSide);
                }

                return stream.ToArray();
            }
        }
    }
}
