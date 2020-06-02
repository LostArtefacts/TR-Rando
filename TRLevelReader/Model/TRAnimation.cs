using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRAnimation
    {
        public uint FrameOffset { get; set; }

        public byte FrameRate { get; set; }

        public byte FrameSize { get; set; }

        public ushort StateID { get; set; }

        //fixed Speed - 4 bytes (2 for whole 2 for frac);
        public FixedFloat32 Speed { get; set; }

        //fixed Accel - 4 bytes (2 for whole 2 for frac);
        public FixedFloat32 Accel { get; set; }

        public ushort FrameStart { get; set; }

        public ushort FrameEnd { get; set; }

        public ushort NextAnimation { get; set; }

        public ushort NextFrame { get; set; }

        public ushort NumStateChanges { get; set; }

        public ushort StateChangeOffset { get; set; }

        public ushort NumAnimCommands { get; set; }

        public ushort AnimCommand { get; set; }
    }
}
