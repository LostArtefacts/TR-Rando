using TRGE.Core;

namespace TRRandomizerCore.Randomizers;

public class TR2RAudioRandomizer : BaseTR2RRandomizer
{
    private const int _numSamples = 412;

    public override void Randomize(int seed)
    {
        TR2AudioAllocator allocator = new(ScriptEditor.AudioProvider.GetCategorisedTracks(), _numSamples)
        {
            Generator = new(seed),
            Settings = Settings,
        };
        allocator.Initialise(Levels.Select(l => l.LevelFileBaseName), BackupPath);

        foreach (TRRScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            allocator.RandomizeMusicTriggers(_levelInstance.Data);
            allocator.RandomizeSoundEffects(_levelInstance.Name, _levelInstance.Data);
            allocator.RandomizePitch(_levelInstance.Data);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }
}
