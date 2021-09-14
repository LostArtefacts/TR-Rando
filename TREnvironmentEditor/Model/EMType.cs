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
        AddStaticMesh = 24,
        RemoveStaticMesh = 25,

        // Entity types 41-60
        MoveSlot = 41,
        MoveEnemy = 42,
        MovePickup = 43,
        MoveEntity = 44,
        ConvertEntity = 45,
        MoveTrap = 46,

        // Trigger types 61-80
        Trigger = 61,
        RemoveTrigger = 62,
        DuplicateTrigger = 63,
        DuplicateSwitchTrigger = 64,
        CameraTriggerFunction = 65,
        ReplaceTriggerActionParameterFunction = 66,
        MoveTrigger = 67,

        // Portal types 81-100
        VisibilityPortal = 81,
        CollisionalPortal = 82,

        // Sound types 101+
        AddSoundSource = 101,
        MoveSoundSource = 102,
        RemoveSoundSource = 103,

        // NOOP/Placeholder
        NOOP = 1000
    }
}