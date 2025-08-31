using TRDataControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors.Shared;

namespace TRRandomizerCore.Processors;

public class TR2XPreProcessor : TR2LevelProcessor
{
    private static readonly Version _minTR2XVersion = new(1, 4);
    private static readonly List<TR2Type> _injectReplaceTypes =
    [
        TR2Type.Map_M_U, TR2Type.FontGraphics_S_H,
    ];

    public void Run()
    {
        var photoSfx = LoadLevelData(TR2LevelNames.GW).SoundEffects[TR2SFX.MenuLaraHome];

        var commonProcessor = new TRXCommonProcessor(ScriptEditor, _minTR2XVersion);
        commonProcessor.AdjustScript();

        Parallel.ForEach(Levels, (scriptedLevel, state) =>
        {
            commonProcessor.AdjustInjections(scriptedLevel);

            var level = LoadCombinedLevel(scriptedLevel);
            ImportData(level, photoSfx);
            if (level.HasCutScene)
            {
                ImportData(level, photoSfx);
            }

            SaveLevel(level);
            if (!TriggerProgress())
            {
                state.Break();
            }
        });

        SaveScript();
    }

    private static void ImportData(TR2CombinedLevel level, TR2SoundEffect photoSfx)
    {
        level.Data.Models.RemoveAll(_injectReplaceTypes.Contains);
        level.Data.Sprites.RemoveAll(_injectReplaceTypes.Contains);

        var importer = new TR2DataImporter(isCommunityPatch: true)
        {
            DataFolder = "Resources/TR2/Objects",
            Level = level.Data,
            TypesToImport = [.. _injectReplaceTypes],
        };

        importer.Import();

        if (photoSfx != null)
        {
            level.Data.SoundEffects[TR2SFX.MenuLaraHome] = photoSfx;
        }
    }
}
