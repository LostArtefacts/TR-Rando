using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelControl.Helpers
{
    public static class TextileToBitmapConverter
    {
        public static byte To32BPP(byte input)
        {
            return Convert.ToByte(input * 255 / 31);
        }

        public static byte From32BPP(byte input)
        {
            return Convert.ToByte(input * 31 / 255);
        }
    }
}
