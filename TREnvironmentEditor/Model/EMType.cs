namespace TREnvironmentEditor.Model
{
    public enum EMType
    {
        // Surface types 0-20
        Ladder = 0,
        Floor = 1,
        Flood = 2,
        Drain = 3,
        Ceiling = 4,

        // Texture types 21-40
        Reface = 21,
        RemoveFace = 22,
        ModifyFace = 23,

        // Entity types 41-60
        MoveSlot = 41,
        MoveEnemy = 42,
        MovePickup = 43,
        MoveEntity = 44,

        // Trigger types 61-80
        Trigger = 61,
        RemoveTrigger = 62,
        DuplicateTrigger = 63,
        DuplicateSwitchTrigger = 64,
        CameraTriggerFunction = 65,

        // Portal types 81+
        VisibilityPortal = 81,
        CollisionalPortal = 82,

        // NOOP/Placeholder
        NOOP = 1000
    }
}