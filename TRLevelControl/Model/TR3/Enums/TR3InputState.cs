namespace TRLevelControl.Model;

[Flags]
public enum TR3InputState
{
    None        = 0,
    Forward     = 1 << 0,
    Back        = 1 << 1,
    Left        = 1 << 2,
    Right       = 1 << 3,
    Jump        = 1 << 4,
    Draw        = 1 << 5,
    Action      = 1 << 6,
    Walk        = 1 << 7,
    Option      = 1 << 8,
    Look        = 1 << 9,
    StepLeft    = 1 << 10,
    StepRight   = 1 << 11,
    Roll        = 1 << 12,
    Flare       = 1 << 19,
    MenuConfirm = 1 << 20,
    MenuBack    = 1 << 21,
    Save        = 1 << 22,
    Load        = 1 << 23,
    Duck        = 1 << 30,
    Sprint      = 1 << 31,
}
