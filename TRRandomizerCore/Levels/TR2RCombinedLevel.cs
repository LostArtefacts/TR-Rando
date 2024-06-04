using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Levels;

public class TR2RCombinedLevel
{
    public TR2Level Data { get; set; }
    public TRRScriptedLevel Script { get; set; }
    public string Checksum { get; set; }
    public string Name => Script.LevelFileBaseName.ToUpper();
    public TR2RCombinedLevel CutSceneLevel { get; set; }
    public TR2RCombinedLevel ParentLevel { get; set; }
    public bool IsCutScene => ParentLevel != null;
    public bool HasCutScene => Script.HasCutScene;
    public int Sequence => IsCutScene ? ParentLevel.Sequence : Script.Sequence;
    public bool Is(string levelFileName) => Script.Is(levelFileName);
    public bool IsAssault => Is(TR2LevelNames.ASSAULT);
    public TRDictionary<TR2Type, TRModel> PDPData { get; set; }
    public Dictionary<TR2Type, TR2RAlias> MapData { get; set; }
    public TRGData TRGData { get; set; }
}
