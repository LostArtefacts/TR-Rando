namespace TRLevelControl.Model;

[Flags]
public enum TRFlybyFlags
{
    None           = 0,
    Snap           = 1 << 0,
    Vignette       = 1 << 1,
    Loop           = 1 << 2,
    Track          = 1 << 3,
    HideLara       = 1 << 4,
    TargetLara     = 1 << 5,
    SnapBack       = 1 << 6,
    JumpTo         = 1 << 7,
    Hold           = 1 << 8,
    NoBreak        = 1 << 9,
    LaraControlOff = 1 << 10,
    LaraControlOn  = 1 << 11,
    FadeInScreen   = 1 << 12,
    FadeOutScreen  = 1 << 13,
    TestTriggers   = 1 << 14,
    OneShot        = 1 << 15
}
