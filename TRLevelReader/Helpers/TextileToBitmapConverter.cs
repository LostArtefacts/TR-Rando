using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Helpers
{
    internal static class TextileToBitmapConverter
    {
        internal static byte To32BPP(byte input)
        {
            return Convert.ToByte((input / 31) * (255));
        }

        internal static byte From32BPP(byte input)
        {
            return Convert.ToByte((input / 255) * (31));
        }
    }
}
