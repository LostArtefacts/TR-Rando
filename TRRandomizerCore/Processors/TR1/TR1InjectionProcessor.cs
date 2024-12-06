using TRGE.Core;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors;

public class TR1InjectionProcessor : TR1LevelProcessor
{
    private static readonly uint _tr1xMagic = 'T' | ('1' << 8) | ('M' << 16) | ('J' << 24);
    private static readonly Version _minTR1XVersion = new(3, 0);
    private static readonly List<TR1XInjectionType> _permittedInjections = new()
    {
        TR1XInjectionType.LaraAnims,
        TR1XInjectionType.LaraJumps,
        TR1XInjectionType.Skybox,
        TR1XInjectionType.PSCrystal,
    };

    public void Run()
    {
        TR1Script script = ScriptEditor.Script as TR1Script;

        bool tr1xVersionSupported = script.Edition.ExeVersion != null
            && script.Edition.ExeVersion >= _minTR1XVersion;
        script.Injections = tr1xVersionSupported ?
            GetSupportedInjections(script.Injections) : null;

        foreach (TR1ScriptedLevel level in Levels)
        {
            LoadLevelInstance(level);
            AdjustInjections(_levelInstance, tr1xVersionSupported);
            SaveLevelInstance();

            if (!TriggerProgress())
            {
                return;
            }
        }

        SaveScript();
    }

    private void AdjustInjections(TR1CombinedLevel level, bool tr1xVersionSupported)
    {
        if (!tr1xVersionSupported)
        {
            level.Script.ResetInjections();
            return;
        }

        level.Script.Injections = GetSupportedInjections(level.Script.Injections);
        if (level.HasCutScene)
        {
            TR1ScriptedLevel cutscene = level.CutSceneLevel.Script;
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

        List<string> validInjections = new();
        foreach (string injection in injections)
        {
            string path = Path.Combine(ScriptEditor.OriginalFile.DirectoryName, "../", injection);
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

        return validInjections.ToArray();
    }
}
