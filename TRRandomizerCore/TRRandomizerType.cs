namespace TRRandomizerCore
{
    public enum TRRandomizerType
    {
        All,

        // Top-level options
        LevelSequence,
        Unarmed,
        Ammoless,
        Sunset,
        NightMode,
        Secret,
        SecretReward,
        Item,
        Enemy,
        Texture,
        StartPosition,
        Audio,
        Outfit,
        Text,
        Environment,
        Health, // Distinct from ammoless in Tomb1Main
        Weather,

        // Individual settings
        DisableDemos = 100,
        OutfitDagger,
        SFX,
        GlobeDisplay,
        RewardRooms,
        VFX,
        MaximumDragons,
        BirdMonsterBehaviour,
        SecretAudio,
        Mediless,
        KeyItems,
        GlitchedSecrets,
        HardSecrets
    }
}