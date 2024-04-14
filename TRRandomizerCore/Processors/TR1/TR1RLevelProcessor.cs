using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors;

public class TR1RLevelProcessor : AbstractLevelProcessor<TRRScriptedLevel, TR1RCombinedLevel>
{
    protected TR1LevelControl _control;

    public TR1RLevelProcessor()
    {
        _control = new();
    }

    protected override TR1RCombinedLevel LoadCombinedLevel(TRRScriptedLevel scriptedLevel)
    {
        TR1RCombinedLevel level = new()
        {
            Data = LoadLevelData(scriptedLevel.LevelFileBaseName),
            Script = scriptedLevel,
            Checksum = GetBackupChecksum(scriptedLevel.LevelFileBaseName)
        };

        if (scriptedLevel.HasCutScene)
        {
            level.CutSceneLevel = LoadCombinedLevel(scriptedLevel.CutSceneLevel as TRRScriptedLevel);
            level.CutSceneLevel.ParentLevel = level;
        }

        return level;
    }

    public TR1Level LoadLevelData(string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            return _control.Read(fullPath);
        }
    }

    protected override void ReloadLevelData(TR1RCombinedLevel level)
    {
        level.Data = LoadLevelData(level.Name);
        if (level.HasCutScene)
        {
            level.CutSceneLevel.Data = LoadLevelData(level.CutSceneLevel.Name);
        }
    }

    protected override void SaveLevel(TR1RCombinedLevel level)
    {
        SaveLevel(level.Data, level.Name);
        if (level.HasCutScene)
        {
            SaveLevel(level.CutSceneLevel.Data, level.CutSceneLevel.Name);
        }

        SaveScript();
    }

    public void SaveLevel(TR1Level level, string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            _control.Write(level, fullPath);
        }
    }
}
