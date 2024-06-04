using TRLevelControl.Model;

namespace TRRandomizerCore.Randomizers;

public class TR2RTextureRandomizer : BaseTR2RRandomizer
{
    public override void Randomize(int seed)
    {
        TextureAllocator allocator = new()
        {
            Generator = new(seed),
            Settings = Settings,
        };

        allocator.LoadData(Levels, level =>
        {
            TRGData data = LoadTRGData(level.TrgFileBaseName);
            return TriggerProgress() ? data : null;
        });

        if (SaveMonitor.IsCancelled)
        {
            return;
        }

        allocator.Allocate(TRGameVersion.TR2, (level, data) =>
        {
            SaveTRGData(data, level.TrgFileBaseName);
            return TriggerProgress();
        });
    }
}
