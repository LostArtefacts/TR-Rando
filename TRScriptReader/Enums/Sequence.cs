using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRScriptReader.Enums
{
    public enum Sequence: ushort
    {
        Picture,
        ListStart,
        ListEnd,
        Fmv,
        Level,
        Cine,
        Complete,
        Demo,
        JumpToSequence,
        End,
        Track,
        Sunset, /* Bartoli's */
        LoadPic, /* TR2 PSX / TR3 */
        DeadlyWater, /* Not used */
        RemoveWeapons,
        GameComplete,
        CutAngle,
        NoFloor,
        StartInv,
        StartAnim,
        Secrets,
        KillToComplete,
        Bonus, /* Same as StartInv, but item IDs are 1000+ */
    }
}
