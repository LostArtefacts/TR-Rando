using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRSoundSource
    {
        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        public ushort SoundID { get; set; }

        public ushort Flags { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" X: " + X);
            sb.Append(" Y: " + Y);
            sb.Append(" Z: " + Z);
            sb.Append(" SoundID: " + SoundID);
            sb.Append(" Flags: " + Flags.ToString("X4"));

            return sb.ToString();
        }
    }
}
