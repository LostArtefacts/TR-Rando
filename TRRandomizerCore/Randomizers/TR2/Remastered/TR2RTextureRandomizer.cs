using TRGE.Core;
using TRLevelControl.Model;

namespace TRRandomizerCore.Randomizers;

public class TR2RTextureRandomizer : BaseTR2RRandomizer
{
    public override void Randomize(int seed)
    {
        TextureAllocator<TR2Type, TR2RAlias> allocator = new(TRGameVersion.TR2)
        {
            Generator = new(seed),
            Settings = Settings,
        };

        foreach (TRRScriptedLevel level in Levels)
        {
            TRGData trgData = LoadTRGData(level.TrgFileBaseName);
            Dictionary<TR2Type, TR2RAlias> mapData = LoadMapData(level.MapFileBaseName);
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
