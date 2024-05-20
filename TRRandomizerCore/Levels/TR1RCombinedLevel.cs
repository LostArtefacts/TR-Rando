using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Levels;

public class TR1RCombinedLevel
{
    public TR1Level Data { get; set; }
    public TRRScriptedLevel Script { get; set; }
    public string Checksum { get; set; }
    public string Name => Script.LevelFileBaseName.ToUpper();
    public TR1RCombinedLevel CutSceneLevel { get; set; }
    public TR1RCombinedLevel ParentLevel { get; set; }
    public bool IsCutScene => ParentLevel != null;
    public bool HasCutScene => Script.HasCutScene;
    public int Sequence => IsCutScene ? ParentLevel.Sequence : Script.Sequence;
    public bool Is(string levelFileName) => Script.Is(levelFileName);
    public bool IsAssault => Is(TR1LevelNames.ASSAULT);
    public TRDictionary<TR1Type, TRModel> PDPData { get; set; }
    public Dictionary<TR1Type, TR1RAlias> MapData { get; set; }
}
