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
        Click = 5,

        // Texture types 21-40
        Reface = 21,
        RemoveFace = 22,
        ModifyFace = 23,
        AddStaticMesh = 24,
        RemoveStaticMesh = 25,
        AddFace = 26,
        MirrorStaticMesh = 27,
        MirrorObjectTexture = 28,

        // Entity types 41-60
        MoveSlot = 41,
        MoveEnemy = 42,
        MovePickup = 43,
        MoveEntity = 44,
        ConvertEntity = 45,
        MoveTrap = 46,
        ConvertEnemy = 47,
        ModifyEntity = 48,
        SwapSlot = 49,
        AdjustEntityPositions = 50,
        AddEntity = 51,
        ConvertWheelDoor = 52,

        // Trigger types 61-80
        Trigger = 61,
        RemoveTrigger = 62,
        DuplicateTrigger = 63,
        DuplicateSwitchTrigger = 64,
        CameraTriggerFunction = 65,
        ReplaceTriggerActionParameterFunction = 66,
        MoveTrigger = 67,
        AppendTriggerActionFunction = 68,
        ConvertTrigger = 69,
        KillLara = 70,
        RemoveTriggerAction = 71,

        // Portal types 81-100
        VisibilityPortal = 81,
        HorizontalCollisionalPortal = 82,
        VerticalCollisionalPortal = 83,
        AdjustVisibilityPortal = 84,

        // Sound types 101-120
        AddSoundSource = 101,
        MoveSoundSource = 102,
        RemoveSoundSource = 103,

        // Room types 121-140
        ModifyRoom = 121,
        ModifyOverlaps = 122,
        CopyRoom = 123,
        CopyVertexAttributes = 124,
        ImportRoom = 125,

        // Models
        ImportModel = 141,
        MirrorModel = 142,
        ConvertSpriteSequence = 143,
        ConvertModel = 144,

        // NOOP/Placeholder
        NOOP = 1000
    }
}