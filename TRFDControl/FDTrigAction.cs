using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl
{
    public enum FDTrigAction
    {
        Object = 0x00,
        Camera = 0x01,
        UnderwaterCurrent = 0x02,
        FlipMap = 0x03,
        FlipOn = 0x04,
        FlipOff = 0x05,
        LookAtItem = 0x06,
        EndLevel = 0x07,
        PlaySoundtrack = 0x08,
        Flipeffect = 0x09,
        SecretFound = 0x0A,
        ClearBodies = 0x0B,
        Flyby = 0x0C,
        Cutscene = 0x0D
    }
}
