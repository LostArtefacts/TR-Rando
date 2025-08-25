using TRRandomizerCore.Levels;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRRandomizerCore.Processors;

public class TR2LevelProcessor : AbstractLevelProcessor<TRXScriptedLevel, TR2CombinedLevel>
{
    protected TR2LevelControl _control;

    public TR2LevelProcessor()
    {
        _control = new();
    }

    protected override TR2CombinedLevel LoadCombinedLevel(TRXScriptedLevel scriptedLevel)
    {
        TR2CombinedLevel level = new()
        {
            Data = LoadLevelData(scriptedLevel.LevelFileBaseName),
            Script = scriptedLevel,
            Checksum = GetBackupChecksum(scriptedLevel.LevelFileBaseName)
        };

        if (scriptedLevel.HasCutScene)
        {
            level.CutSceneLevel = LoadCombinedLevel(scriptedLevel.CutSceneLevel as TRXScriptedLevel);
            level.CutSceneLevel.ParentLevel = level;
        }

        return level;
    }

    public TR2Level LoadLevelData(string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            return _control.Read(fullPath);
        }
    }

    protected override void ReloadLevelData(TR2CombinedLevel level)
    {
        level.Data = LoadLevelData(level.Name);
        if (level.HasCutScene)
        {
            level.CutSceneLevel.Data = LoadLevelData(level.CutSceneLevel.Name);
        }
    }

    protected override void SaveLevel(TR2CombinedLevel level)
    {
        SaveLevel(level.Data, level.Name);
        if (level.HasCutScene)
        {
            SaveLevel(level.CutSceneLevel.Data, level.CutSceneLevel.Name);
        }

        SaveScript();
    }

    public void SaveLevel(TR2Level level, string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            _control.Write(level, fullPath);
        }
    }
}
