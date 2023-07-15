using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelControl
{
    internal static class UnsafeConversions
    {
        public static short UShortToShort(ushort val)
        {
            return unchecked((short)val);
        }

        public static int UIntToInt(uint val)
        {
            return unchecked((int)val);
        }
    }
}
