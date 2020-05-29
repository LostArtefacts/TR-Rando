using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRCinematicFrame
    {
        public short TargetX { get; set; }

        public short TargetY { get; set; }

        public short TargetZ { get; set; }

        public short PosZ { get; set; }

        public short PosY { get; set; }

        public short PosX { get; set; }

        public short FOV { get; set; }

        public short Roll { get; set; }
    }
}
