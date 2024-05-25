using TRGE.Core;

namespace TRRandomizerCore.Randomizers;

public class TR2AudioRandomizer : BaseTR2Randomizer
{
    private const int _numSamples = 408; // Number of entries in MAIN.SFX

    public override void Randomize(int seed)
    {
        TR2AudioAllocator allocator = new(ScriptEditor.AudioProvider.GetCategorisedTracks(), _numSamples)
        {
            Generator = new(seed),
            Settings = Settings,
        };
        allocator.Initialise(Levels.Select(l => l.LevelFileBaseName), BackupPath);

        foreach (TR2ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            allocator.RandomizeMusicTriggers(_levelInstance.Data);
            allocator.RandomizeSoundEffects(_levelInstance.Name, _levelInstance.Data);
            allocator.RandomizePitch(_levelInstance.Data.SoundEffects.Values);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }
}
