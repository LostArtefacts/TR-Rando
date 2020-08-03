using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRScriptReader.Enums
{
    [Flags]
    public enum Flag: ushort
    {
        DemoVersion = 0x1,
        TitleDisabled = 0x2,
        CheatModeCheckDisabled = 0x4,
        NoInputTimeout = 0x8,
        LoadSaveDisabled = 0x10,
        ScreenSizingDisabled = 0x20,
        LockoutOptionRing = 0x40,
        DozyCheatEnabled = 0x80,
        UseXor = 0x100,
        GymEnabled = 0x200,
        SelectAnyLevel = 0x400,
        EnableCheatCode = 0x800,
    }
}
