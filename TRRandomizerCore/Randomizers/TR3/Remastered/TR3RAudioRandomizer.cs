using TRGE.Core;

namespace TRRandomizerCore.Randomizers;

public class TR3RAudioRandomizer : BaseTR3RRandomizer
{
    public override void Randomize(int seed)
    {
        TR3AudioAllocator allocator = new(ScriptEditor.AudioProvider.GetCategorisedTracks())
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
            allocator.RandomizePitch(_levelInstance.Data.SoundEffects.Values);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }
}
