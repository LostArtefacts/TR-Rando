using TRGE.Core;
using TRLevelControl.Model;

namespace TRRandomizerCore.Randomizers;

public class TR1RTextureRandomizer : BaseTR1RRandomizer
{
    public override void Randomize(int seed)
    {
        TextureAllocator<TR1Type, TR1RAlias> allocator = new(TRGameVersion.TR1)
        {
            Generator = new(seed),
            Settings = Settings,
        };

        foreach (TRRScriptedLevel level in Levels)
        {
            TRGData trgData = LoadTRGData(level.TrgFileBaseName);
            Dictionary<TR1Type, TR1RAlias> mapData = LoadMapData(level.MapFileBaseName);
            allocator.LoadData(level, trgData, mapData);
            if (!TriggerProgress())
            {
                return;
            }
        }

        allocator.Allocate((level, trgData, mapData) =>
        {
            SaveMapData(mapData, level.MapFileBaseName);
            SaveTRGData(trgData, level.TrgFileBaseName);
            return TriggerProgress();
        });
    }
}
