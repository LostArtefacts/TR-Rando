using TRDataControl;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors;

public class TR1InjectionProcessor : TR1LevelProcessor
{
    private static readonly uint _tr1xMagic = 'T' | ('R' << 8) | ('X' << 16) | ('J' << 24);
    private static readonly Version _minTR1XVersion = new(4, 14);

    private static readonly List<TR1Type> _injectReplaceTypes = [TR1Type.Map_M_U, TR1Type.FontGraphics_S_H, TR1Type.PickupAid_S_H];
    private static readonly List<TR1XInjectionType> _permittedInjections =
    [
        TR1XInjectionType.LaraAnims,
        TR1XInjectionType.Skybox,
        TR1XInjectionType.PSCrystal,
    ];

    public void Run()
    {
        var photoSfx = LoadLevelData(TR1LevelNames.CAVES).SoundEffects[TR1SFX.MenuChoose];
        var script = ScriptEditor.Script as TRXScript;

        bool tr1xVersionSupported = script.Edition.ExeVersion != null
            && script.Edition.ExeVersion >= _minTR1XVersion;
        script.Injections = tr1xVersionSupported ?
            GetSupportedInjections(script.Injections) : null;

        Parallel.ForEach(Levels, (scriptedLevel, state) =>
        {
            AdjustInjections(scriptedLevel, tr1xVersionSupported);

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

    private void AdjustInjections(TRXScriptedLevel level, bool tr1xVersionSupported)
    {
        if (!tr1xVersionSupported)
        {
            level.ResetInjections();
            return;
        }

        level.Injections = GetSupportedInjections(level.Injections);
        if (level.HasCutScene)
        {
            var cutscene = (level.CutSceneLevel as TRXScriptedLevel);
            cutscene.InheritInjections = false;
            cutscene.Injections = GetSupportedInjections(cutscene.Injections);
        }
    }

    private string[] GetSupportedInjections(string[] injections)
    {
        if (injections == null)
        {
            return injections;
        }

        List<string> validInjections = [];
        foreach (string injection in injections)
        {
            string path = Path.Combine(ScriptEditor.OriginalFile.DirectoryName, "../../", injection);
            if (!File.Exists(path))
            {
                continue;
            }

            using BinaryReader reader = new(File.OpenRead(path));
            if (reader.ReadUInt32() != _tr1xMagic)
            {
                continue;
            }

            reader.ReadUInt32(); // Skip version
            if (_permittedInjections.Contains((TR1XInjectionType)reader.ReadUInt32()))
            {
                validInjections.Add(injection);
            }
        }

        return [.. validInjections];
    }

    private static void ImportData(TR1CombinedLevel level, TR1SoundEffect photoSfx)
    {
        // Clear out objects that would normally be replaced by TRX injections and import
        // them as standard to allow targeting in other rando classes.
        level.Data.Models.Remove(TR1Type.LaraPonytail_H_U);
        level.Data.Models.RemoveAll(_injectReplaceTypes.Contains);
        level.Data.Sprites.RemoveAll(_injectReplaceTypes.Contains);

        var importer = new TR1DataImporter
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
