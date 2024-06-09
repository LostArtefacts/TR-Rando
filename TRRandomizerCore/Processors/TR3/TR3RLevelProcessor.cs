using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors;

public class TR3RLevelProcessor : AbstractLevelProcessor<TRRScriptedLevel, TR3RCombinedLevel>
{
    protected TR3LevelControl _levelControl;
    protected TR3PDPControl _pdpControl;
    protected TR3MapControl _mapControl;

    public TR3RLevelProcessor()
    {
        _levelControl = new();
        _pdpControl = new();
        _mapControl = new();
    }

    protected override TR3RCombinedLevel LoadCombinedLevel(TRRScriptedLevel scriptedLevel)
    {
        TR3RCombinedLevel level = new()
        {
            Data = LoadLevelData(scriptedLevel.LevelFileBaseName),
            PDPData = LoadPDPData(scriptedLevel.PdpFileBaseName),
            MapData = LoadMapData(scriptedLevel.MapFileBaseName),
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
            return _levelControl.Read(fullPath);
        }
    }

    public TRDictionary<TR3Type, TRModel> LoadPDPData(string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            return File.Exists(fullPath) ? _pdpControl.Read(fullPath) : null;
        }
    }

    public Dictionary<TR3Type, TR3RAlias> LoadMapData(string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            return File.Exists(fullPath) ? _mapControl.Read(fullPath) : null;
        }
    }

    protected override void ReloadLevelData(TR3RCombinedLevel level)
    {
        level.Data = LoadLevelData(level.Name);
        level.PDPData = LoadPDPData(level.Script.PdpFileBaseName);
        level.MapData = LoadMapData(level.Script.MapFileBaseName);
        if (level.TRGData != null)
        {
            level.TRGData = LoadTRGData(level.Script.TrgFileBaseName);
        }

        if (level.HasCutScene)
        {
            level.CutSceneLevel.Data = LoadLevelData(level.CutSceneLevel.Name);
            level.CutSceneLevel.PDPData = LoadPDPData(level.CutSceneLevel.Script.PdpFileBaseName);
            level.CutSceneLevel.MapData = LoadMapData(level.CutSceneLevel.Script.MapFileBaseName);
            if (level.CutSceneLevel.TRGData != null)
            {
                level.CutSceneLevel.TRGData = LoadTRGData(level.CutSceneLevel.Script.TrgFileBaseName);
            }
        }
    }

    protected override void SaveLevel(TR3RCombinedLevel level)
    {
        SaveLevel(level.Data, level.Name);
        SavePDPData(level.PDPData, level.Script.PdpFileBaseName);
        SaveMapData(level.MapData, level.Script.MapFileBaseName);
        SaveTRGData(level.TRGData, level.Script.TrgFileBaseName);
        if (level.HasCutScene)
        {
            SaveLevel(level.CutSceneLevel.Data, level.CutSceneLevel.Name);
            SavePDPData(level.CutSceneLevel.PDPData, level.CutSceneLevel.Script.PdpFileBaseName);
            SaveMapData(level.CutSceneLevel.MapData, level.CutSceneLevel.Script.MapFileBaseName);
            SaveTRGData(level.CutSceneLevel.TRGData, level.CutSceneLevel.Script.TrgFileBaseName);
        }

        SaveScript();
    }

    public void SaveLevel(TR3Level level, string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            _levelControl.Write(level, fullPath);
        }
    }

    public void SavePDPData(TRDictionary<TR3Type, TRModel> data, string name)
    {
        if (data == null)
        {
            return;
        }

        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            _pdpControl.Write(data, fullPath);
        }
    }

    public void SaveMapData(Dictionary<TR3Type, TR3RAlias> data, string name)
    {
        if (data == null)
        {
            return;
        }

        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            _mapControl.Write(data, fullPath);
        }
    }
}
