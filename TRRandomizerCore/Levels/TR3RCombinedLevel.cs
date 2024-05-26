using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Levels;

public class TR3RCombinedLevel
{
    public TR3Level Data { get; set; }
    public TRRScriptedLevel Script { get; set; }
    public string Checksum { get; set; }
    public string Name => Script.LevelFileBaseName.ToUpper();
    public TR3RCombinedLevel CutSceneLevel { get; set; }
    public TR3RCombinedLevel ParentLevel { get; set; }
    public bool IsCutScene => ParentLevel != null;
    public bool HasCutScene => Script.HasCutScene;
    public int Sequence => IsCutScene ? ParentLevel.Sequence : Script.Sequence;
    public bool Is(string levelFileName) => Script.Is(levelFileName);
    public bool IsAssault => Is(TR3LevelNames.ASSAULT);
    public TRDictionary<TR3Type, TRModel> PDPData { get; set; }
    public Dictionary<TR3Type, TR3RAlias> MapData { get; set; }
    public bool HasExposureMeter => Sequence == 16 || Sequence == 17;
}
