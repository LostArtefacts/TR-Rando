using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRScriptReader.Enums
{
    public enum Command: int
    {
        Level = 0x0,
        SavedGame = 0x100,
        Cutscene = 0x200,
        FMV = 0x300,
        Demo = 0x400,
        ExitToTitle = 0x500,
        ExitGame = 0x700,
        NoOp = -1,
    }
}