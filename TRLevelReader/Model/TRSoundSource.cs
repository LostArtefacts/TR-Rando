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
    }
}
