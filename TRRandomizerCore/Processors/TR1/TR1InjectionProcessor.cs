using System;
using System.Collections.Generic;
using System.IO;
using TRGE.Core;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors;

public class TR1InjectionProcessor : TR1LevelProcessor
{
    private static readonly uint _t1mMagic = 'T' | ('1' << 8) | ('M' << 16) | ('J' << 24);
    private static readonly Version _minT1MVersion = new Version(2, 15);
    private static readonly List<T1MInjectionType> _permittedInjections = new List<T1MInjectionType>
    {
        T1MInjectionType.LaraAnims,
        T1MInjectionType.LaraJumps,
    };

    public void Run()
    {
        TR1Script script = ScriptEditor.Script as TR1Script;

        bool t1mVersionSupported = script.Edition.ExeVersion != null
            && script.Edition.ExeVersion >= _minT1MVersion;
        script.Injections = t1mVersionSupported ?
            GetSupportedInjections(script.Injections) : null;

        foreach (TR1ScriptedLevel level in Levels)
        {
            LoadLevelInstance(level);
            AdjustInjections(_levelInstance, t1mVersionSupported);
            SaveLevelInstance();

            if (!TriggerProgress())
            {
                return;
            }
        }

        SaveScript();
    }

    private void AdjustInjections(TR1CombinedLevel level, bool t1mVersionSupported)
    {
        if (!t1mVersionSupported)
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

        List<string> validInjections = new List<string>();
        foreach (string injection in injections)
        {
            string path = Path.Combine(ScriptEditor.OriginalFile.DirectoryName, @"..\", injection);
            if (!File.Exists(path))
            {
                continue;
            }

            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                if (reader.ReadUInt32() != _t1mMagic)
                {
                    continue;
                }

                reader.ReadUInt32(); // Skip version
                if (_permittedInjections.Contains((T1MInjectionType)reader.ReadUInt32()))
                {
                    validInjections.Add(injection);
                }
            }
        }

        return validInjections.ToArray();
    }
}
