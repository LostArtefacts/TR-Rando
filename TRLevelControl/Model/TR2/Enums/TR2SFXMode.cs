namespace TRLevelControl.Model;

public enum TR2SFXMode : byte
{
    Wait    = 0, // Repeated SFX of the same type will be queued
    Restart = 1, // Repeated SFX of the same type will replace existing
    Ambient = 3, // Looping
}
