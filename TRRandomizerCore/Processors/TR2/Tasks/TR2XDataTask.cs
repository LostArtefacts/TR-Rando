using TRDataControl;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors.TR2.Tasks;

public class TR2XDataTask : ITR2ProcessorTask
{
    private static readonly List<TR2Type> _injectReplaceTypes =
    [
        TR2Type.Map_M_U, TR2Type.FontGraphics_S_H,
    ];

    public void Run(TR2CombinedLevel level)
    {
        ImportData(level);
        FixCutsceneSFX(level);
    }

    private static void ImportData(TR2CombinedLevel level)
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
    }

    private static void FixCutsceneSFX(TR2CombinedLevel level)
    {
        if (level.ParentLevel == null)
        {
            return;
        }

        const TR2SFX sfxId = TR2SFX.MenuLaraHome;
        level.Data.SoundEffects[sfxId] = level.ParentLevel.Data.SoundEffects[sfxId];
    }
}
