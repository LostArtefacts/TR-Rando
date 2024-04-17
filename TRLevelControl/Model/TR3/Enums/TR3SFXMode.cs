namespace TRLevelControl.Model;

public enum TR3SFXMode : byte
{
    Wait        = 0, // Repeated SFX of the same type will be queued
    Restart     = 1, // Repeated SFX of the same type will replace existing
    OneShotWait = 2, // Ignored if it follows one of the same type
    Ambient     = 3, // Looping
}
