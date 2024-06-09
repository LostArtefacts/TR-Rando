using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors;

public class TR2RLevelProcessor : AbstractLevelProcessor<TRRScriptedLevel, TR2RCombinedLevel>
{
    protected TR2LevelControl _levelControl;
    protected TR2PDPControl _pdpControl;
    protected TR2MapControl _mapControl;

    public TR2RLevelProcessor()
    {
        _levelControl = new();
        _pdpControl = new();
        _mapControl = new();
    }

    protected override TR2RCombinedLevel LoadCombinedLevel(TRRScriptedLevel scriptedLevel)
    {
        TR2RCombinedLevel level = new()
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

    public TR2Level LoadLevelData(string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            return _levelControl.Read(fullPath);
        }
    }

    public TRDictionary<TR2Type, TRModel> LoadPDPData(string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            return File.Exists(fullPath) ? _pdpControl.Read(fullPath) : null;
        }
    }

    public Dictionary<TR2Type, TR2RAlias> LoadMapData(string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            return File.Exists(fullPath) ? _mapControl.Read(fullPath) : null;
        }
    }

    protected override void ReloadLevelData(TR2RCombinedLevel level)
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

    protected override void SaveLevel(TR2RCombinedLevel level)
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

    public void SaveLevel(TR2Level level, string name)
    {
        lock (_controlLock)
        {
            string fullPath = Path.Combine(BasePath, name);
            _levelControl.Write(level, fullPath);
        }
    }

    public void SavePDPData(TRDictionary<TR2Type, TRModel> data, string name)
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

    public void SaveMapData(Dictionary<TR2Type, TR2RAlias> data, string name)
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
