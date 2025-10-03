using TRLevelControl;

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

        foreach (var lvl in Levels)
        {
            LoadLevelInstance(lvl);

            allocator.RandomizeMusicTriggers(_levelInstance.Data);
            allocator.RandomizeSoundEffects(_levelInstance.Name, _levelInstance.Data);
            allocator.RandomizePitch(_levelInstance.Data.SoundEffects.Values);
            TR2AudioAllocator.FixMusicTracks(_levelInstance.Data);

            SaveLevelInstance();
            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    public void EmbedSamples()
    {
        var mainSFX = ReadMainSFX();
        if (mainSFX.Count == 0)
        {
            return;
        }

        Parallel.ForEach(Levels, l =>
        {
            var level = LoadCombinedLevel(l);
            level.Data.TRXData ??= new();

            foreach (var (sfxID, sfx) in level.Data.SoundEffects)
            {
                if (level.Data.TRXData.SFX.Any(s => s.ID == (short)sfxID))
                {
                    continue;
                }

                level.Data.TRXData.SFX.Add(new()
                {
                    ID = (short)sfxID,
                    Chance = sfx.Chance,
                    Flags = sfx.GetFlags(),
                    Volume = sfx.Volume,
                    Data = mainSFX.GetRange((int)sfx.SampleID, sfx.SampleCount),
                });
            }

            SaveLevel(level);
        });
    }

    private List<byte[]> ReadMainSFX()
    {
        var targetDir = Path.GetDirectoryName(ScriptEditor.OriginalFile.FullName);
        var sfxFile = Path.GetFullPath(Path.Combine(targetDir, "../../data/main.sfx"));
        var result = new List<byte[]>(_numSamples);
        if (!File.Exists(sfxFile))
        {
            return result;
        }

        using var reader = new TRLevelReader(File.Open(sfxFile, FileMode.Open));
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            using var stream = new MemoryStream();
            using var writer = new TRLevelWriter(stream);

            var header = reader.ReadUInt32s(11);
            var data = reader.ReadUInt8s(header[10]);
            writer.Write(header);
            writer.Write(data);

            result.Add(stream.ToArray());
        }

        return result;
    }
}
