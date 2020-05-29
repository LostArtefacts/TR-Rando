using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRTexImage8
    {
        public byte[] Pixels { get; set; }

        public TRTexImage8()
        {
            Pixels = new byte[256 * 256];
        }
    }
}
