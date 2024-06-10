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
            allocator.LoadData(level, LoadTRGData(level.TrgFileBaseName), LoadMapData(level.MapFileBaseName));
            if (level.HasCutScene)
            {
                TRRScriptedLevel cutLevel = level.CutSceneLevel as TRRScriptedLevel;
                allocator.LoadCutData(level, LoadTRGData(cutLevel.TrgFileBaseName));
            }
            if (!TriggerProgress())
            {
                return;
            }
        }

        allocator.Allocate((level, trgData, mapData) =>
        {
            SaveMapData(mapData, level.MapFileBaseName);
            SaveTRGData(trgData, level.TrgFileBaseName);
            return level.IsCutscene || TriggerProgress();
        });
    }
}
