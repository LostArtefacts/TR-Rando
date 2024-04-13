using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors;

public class TR3RLevelProcessor : AbstractLevelProcessor<TRRScriptedLevel, TR3RCombinedLevel>
{
    protected TR3LevelControl _control;

    public TR3RLevelProcessor()
    {
        _control = new();
    }

    protected override TR3RCombinedLevel LoadCombinedLevel(TRRScriptedLevel scriptedLevel)
    {
        TR3RCombinedLevel level = new()
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

    public TR3Level LoadLevelData(string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            return _control.Read(fullPath);
        }
    }

    protected override void ReloadLevelData(TR3RCombinedLevel level)
    {
        level.Data = LoadLevelData(level.Name);
        if (level.HasCutScene)
        {
            level.CutSceneLevel.Data = LoadLevelData(level.CutSceneLevel.Name);
        }
    }

    protected override void SaveLevel(TR3RCombinedLevel level)
    {
        SaveLevel(level.Data, level.Name);
        if (level.HasCutScene)
        {
            SaveLevel(level.CutSceneLevel.Data, level.CutSceneLevel.Name);
        }

        SaveScript();
    }

    public void SaveLevel(TR3Level level, string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            _control.Write(level, fullPath);
        }
    }
}
