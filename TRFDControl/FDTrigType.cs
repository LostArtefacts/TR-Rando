using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl;

public enum FDTrigType
{
    Trigger = 0x00,
    Pad = 0x01,
    Switch = 0x02,
    Key = 0x03,
    Pickup = 0x04,
    HeavyTrigger = 0x05,
    Antipad = 0x06,
    Combat = 0x07,
    Dummy = 0x08,
    AntiTrigger = 0x09,
    HeavySwitch = 0x0A,
    HeavyAntiTrigger = 0x0B,
    Monkey = 0x0C,
    Skeleton = 0x0D,
    Tightrope = 0x0E,
    Crawl = 0x0F,
    Climb = 0x10
}
