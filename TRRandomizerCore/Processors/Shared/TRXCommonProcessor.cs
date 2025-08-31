using TRGE.Core;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Processors.Shared;

public class TRXCommonProcessor
{
    private static readonly uint _trxMagic = 'T' | ('R' << 8) | ('X' << 16) | ('J' << 24);
    private static readonly List<TRXInjectionType> _permittedInjections =
    [
        TRXInjectionType.LaraAnims,
        TRXInjectionType.Skybox,
        TRXInjectionType.PSCrystal,
    ];

    private readonly AbstractTRScriptEditor _scriptEditor;
    private readonly bool _versionSupported;

    public TRXCommonProcessor(AbstractTRScriptEditor scriptEditor, Version minVersion)
    {
        _scriptEditor = scriptEditor;
        _versionSupported = scriptEditor.Edition.ExeVersion != null
            && scriptEditor.Edition.ExeVersion >= minVersion;
    }

    public void AdjustScript()
    {
        var script = _scriptEditor.Script as TRXScript;
        script.Injections = GetSupportedInjections(script.Injections);

        script.HiddenConfig.Add("fix_floor_data_issues");
        script.HiddenConfig.Add("fix_item_rots");
        script.HiddenConfig.Add("fix_texture_issues");
    }

    public void AdjustInjections(TRXScriptedLevel level)
    {
        if (!_versionSupported)
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

    public string[] GetSupportedInjections(string[] injections)
    {
        if (!_versionSupported || injections == null)
        {
            return null;
        }

        List<string> validInjections = [];
        foreach (string injection in injections)
        {
            string path = Path.Combine(_scriptEditor.OriginalFile.DirectoryName, "../../", injection);
            if (!File.Exists(path))
            {
                continue;
            }

            using BinaryReader reader = new(File.OpenRead(path));
            if (reader.ReadUInt32() != _trxMagic)
            {
                continue;
            }

            reader.ReadUInt32(); // Skip version
            if (_permittedInjections.Contains((TRXInjectionType)reader.ReadUInt32()))
            {
                validInjections.Add(injection);
            }
        }

        return [.. validInjections];
    }
}
