namespace TRRandomizerCore.SFX
{
    public enum TRSFXGeneralCategory
    {
        // General 0-20
        Unused,
        Misc,

        // Weapons 21-50
        StandardWeaponFiring = 21,
        FastWeaponFiring,
        Ricochet,

        // Explosions/Crashes 51-80
        Explosion = 51,
        Clattering,
        Breaking,

        // Footsteps 81-110
        StandardFootstep = 81,
        HeavyFootstep,

        // Death, damage 111-140
        Death = 111,
        TakingDamage,

        // Breathing 141-170
        Breathing = 141,

        // Grunting 171-200
        Grunting = 171,

        // Growling 201-230
        Growling = 201,

        // Alerting 231-260
        Alerting = 231,

        // Flying, wing flapping 261-280
        Flying = 261,

        // Doors, gates, switches etc
        GeneralDoor = 281,
        DoorOpening,
        DoorClosing,
        GeneralSwitch,
        SwitchUp,
        SwitchDown
    }
}