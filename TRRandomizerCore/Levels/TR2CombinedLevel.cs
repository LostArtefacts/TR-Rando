using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Levels;

public class TR2CombinedLevel
{
    /// <summary>
    /// The main level data stored in the corresponding .TR2 file.
    /// </summary>
    public TR2Level Data { get; set; }

    /// <summary>
    /// The scripting information for the level stored in TOMBPC.dat.
    /// </summary>
    public TRXScriptedLevel Script { get; set; }

    /// <summary>
    /// The checksum of the backed up level file.
    /// </summary>
    public string Checksum { get; set; }

    /// <summary>
    /// The uppercase base file name of the level e.g. KEEL.TR2
    /// </summary>
    public string Name => Script.LevelFileBaseName.ToUpper();

    /// <summary>
    /// The level data for the cutscene at the end of this level, if any.
    /// </summary>
    public TR2CombinedLevel CutSceneLevel { get; set; }

    /// <summary>
    /// A reference to the main level if this is a CutScene level.
    /// </summary>
    public TR2CombinedLevel ParentLevel { get; set; }

    /// <summary>
    /// True if this is a CutScene level, and so has a parent level.
    /// </summary>
    public bool IsCutScene => ParentLevel != null;

    /// <summary>
    /// Whether or not this level has a cutscene at the end.
    /// </summary>
    public bool HasCutScene => Script.HasCutScene;

    /// <summary>
    /// Gets the level's sequence in the game. If this is a CutScene level, this returns the parent sequence.
    /// </summary>
    public int Sequence => IsCutScene ? ParentLevel.Sequence : Script.Sequence;

    /// <summary>
    /// Compares the given file name or path against the base file name of the level (case-insensitive).
    /// </summary>
    public bool Is(string levelFileName) => Script.Is(levelFileName);

    /// <summary>
    /// Determines if the given level is specific to UKBox. This currently applies only to Floating Islands which differs between UKBox and EPC/Multipatch.
    /// </summary>
    // This is a horrible way to store this. Texture data was previously used to detect, but as this can change, we instead make a change to an unreachable sector.
    public bool IsUKBox
    {
        get => Is(TR2LevelNames.FLOATER) && Data.Rooms[165].Sectors[21].BoxIndex == 0;
        set
        {
            if (Is(TR2LevelNames.FLOATER))
            {
                Data.Rooms[165].Sectors[21].BoxIndex = 0;
            }
        }
    }

    public bool IsExpansion => TR2LevelNames.AsListGold.Contains(Name);

    /// <summary>
    /// Returns {Name}-UKBox if this level is for UKBox, otherwise just {Name}.
    /// </summary>
    public string JsonID => IsUKBox ? Name + "-UKBox" : Name;

    /// <summary>
    /// Checks if the current level is the assault course.
    /// </summary>
    public bool IsAssault => Is(TR2LevelNames.ASSAULT);

    public List<TR2Entity> GetEnemyEntities()
    {
        List<TR2Type> allEnemies = TR2TypeUtilities.GetFullListOfEnemies();
        return Data.Entities.FindAll(e => allEnemies.Contains(e.TypeID));
    }
}
