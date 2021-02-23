namespace TR2RandomizerCore.Zones
{
    enum LevelZones : int
    {
        StoneSecretZone = 0,
        JadeSecretZone,
        GoldSecretZone,
        Key1Zone,
        Key2Zone,
        Key3Zone,
        Key4Zone,
        Puzzle1Zone,
        Puzzle2Zone,
        Puzzle3Zone,
        Puzzle4Zone,
        Quest1Zone,
        Quest2Zone,
        ItemZones
    }

    enum ZonePopulationMethod
    {
        SecretsOnly,
        KeyPuzzleQuestOnly,
        GeneralOnly,
        Full
    }
}