using TRDataControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Processors.Shared;

namespace TRRandomizerCore.Processors;

public class TR1XPreProcessor : TR1LevelProcessor
{
    private static readonly Version _minTR1XVersion = new(4, 14);
    private static readonly List<TR1Type> _injectReplaceTypes =
    [
        TR1Type.Map_M_U, TR1Type.FontGraphics_S_H, TR1Type.PickupAid_S_H
    ];

    public void Run()
    {
        var photoSfx = LoadLevelData(TR1LevelNames.CAVES).SoundEffects[TR1SFX.MenuChoose];
        
        var commonProcessor = new TRXCommonProcessor(ScriptEditor, _minTR1XVersion);
        commonProcessor.AdjustScript();

        Parallel.ForEach(Levels, (scriptedLevel, state) =>
        {
            commonProcessor.AdjustInjections(scriptedLevel);

            var level = LoadCombinedLevel(scriptedLevel);
            ImportData(level, photoSfx);
            if (level.HasCutScene)
            {
                ImportData(level.CutSceneLevel, photoSfx);
            }

            SaveLevel(level);
            if (!TriggerProgress())
            {
                state.Break();
            }
        });

        SaveScript();
    }

    private static void ImportData(TR1CombinedLevel level, TR1SoundEffect photoSfx)
    {
        // Clear out objects that would normally be replaced by TRX injections and import
        // them as standard to allow targeting in other rando classes.
        level.Data.Models.Remove(TR1Type.LaraPonytail_H_U);
        level.Data.Models.RemoveAll(_injectReplaceTypes.Contains);
        level.Data.Sprites.RemoveAll(_injectReplaceTypes.Contains);

        var importer = new TR1DataImporter(true)
        {
            DataFolder = "Resources/TR1/Objects",
            Level = level.Data,
            TypesToImport = _injectReplaceTypes,
        };
        importer.Import();

        if (photoSfx != null)
        {
            level.Data.SoundEffects[TR1SFX.MenuChoose] = photoSfx;
        }
    }
}
