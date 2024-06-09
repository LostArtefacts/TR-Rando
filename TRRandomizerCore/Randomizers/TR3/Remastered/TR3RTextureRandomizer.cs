using TRGE.Core;
using TRLevelControl.Model;

namespace TRRandomizerCore.Randomizers;

public class TR3RTextureRandomizer : BaseTR3RRandomizer
{
    public override void Randomize(int seed)
    {
        TextureAllocator<TR3Type, TR3RAlias> allocator = new(TRGameVersion.TR3)
        {
            Generator = new(seed),
            Settings = Settings,
        };

        foreach (TRRScriptedLevel level in Levels)
        {
            TRGData trgData = LoadTRGData(level.TrgFileBaseName);
            Dictionary<TR3Type, TR3RAlias> mapData = LoadMapData(level.MapFileBaseName);
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
