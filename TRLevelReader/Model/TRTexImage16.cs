using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRTexImage16
    {
        public ushort[] Pixels { get; set; }

        public TRTexImage16()
        {
            Pixels = new ushort[256 * 256];
        }
    }
}
