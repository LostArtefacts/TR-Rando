using TRDataControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Processors.TR2.Tasks;

public class TR2XDataTask : ITR2ProcessorTask
{
    private static readonly List<TR2Type> _injectReplaceTypes =
    [
        TR2Type.Map_M_U, TR2Type.FontGraphics_S_H,
    ];

    public required TR2TextureMonitorBroker TextureMonitor { get; set; }

    public void Run(TR2CombinedLevel level)
    {
        ImportData(level);
        FixCutsceneSFX(level);
    }

    private void ImportData(TR2CombinedLevel level)
    {
        level.Data.Models.RemoveAll(_injectReplaceTypes.Contains);
        level.Data.Sprites.RemoveAll(_injectReplaceTypes.Contains);

        var importer = new TR2DataImporter(isCommunityPatch: true)
        {
            DataFolder = "Resources/TR2/Objects",
            Level = level.Data,
            TypesToImport = [.. _injectReplaceTypes],
        };

        if (level.Data.Models.Remove(TR2Type.Boat))
        {
            // Replace the boat and add the restored exploding mesh bits to allow targeting textures
            // during randomization.
            importer.TypesToImport.Add(TR2Type.Boat);
            importer.TextureMonitor = TextureMonitor.CreateMonitor(level.Name, [TR2Type.Boat]);
        }
        else if (level.Is(TR2LevelNames.FLOATER) || level.Is(TR2LevelNames.LAIR))
        {
            importer.TypesToImport.Add(TR2Type.Pistols_S_P);
            importer.TypesToImport.AddRange(TR2TypeUtilities.GetGunTypes());
        }
        else if (level.IsAssault || level.Is(TR2LevelNames.HOME))
        {
            var guns = TR2TypeUtilities.GetGunModels();
            if (level.IsAssault)
            {
                level.Data.Models.Remove(TR2Type.LaraShotgunAnim_H);
            }
            else
            {
                guns.Remove(TR2Type.Shotgun_M_H);
            }
            level.Data.Models.RemoveAll(guns.Contains);
            importer.TypesToImport.AddRange(guns);
        }

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
