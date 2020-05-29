using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRSoundDetails
    {
        public ushort Sample { get; set; }

        public ushort Volume { get; set; }

        public ushort Chance { get; set; }

        public ushort Characteristics { get; set; }
    }
}
