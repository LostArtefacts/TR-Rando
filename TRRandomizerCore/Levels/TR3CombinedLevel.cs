using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Levels;

public class TR3CombinedLevel
{
    /// <summary>
    /// The main level data stored in the corresponding .TR2 file.
    /// </summary>
    public TR3Level Data { get; set; }

    /// <summary>
    /// The scripting information for the level stored in TOMBPC.dat.
    /// </summary>
    public TR3ScriptedLevel Script { get; set; }

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
    public TR3CombinedLevel CutSceneLevel { get; set; }

    /// <summary>
    /// A reference to the main level if this is a CutScene level.
    /// </summary>
    public TR3CombinedLevel ParentLevel { get; set; }

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
    /// Checks if the current level is the assault course.
    /// </summary>
    public bool IsAssault => Is(TR3LevelNames.ASSAULT);

    /// <summary>
    /// The exposure meter is hard-coded to the Antarctica and RX-Tech Mines level sequences.
    /// In tomb3, this is customisable.
    /// </summary>
    public bool HasExposureMeter => Script.HasColdWater;

    /// <summary>
    /// Whether or not this level is in the sequence of original Coastal Village.
    /// </summary>
    public bool IsCoastalSequence => Sequence == 5;

    /// <summary>
    /// Whether or not this level is in the sequence of original Crash Site.
    /// </summary>
    public bool IsCrashSequence => Sequence == 6;

    /// <summary>
    /// Whether or not this level is in the sequence of original Madubu Gorge.
    /// </summary>
    public bool IsMadubuSequence => Sequence == 7;

    /// <summary>
    /// Whether or not this level is in the sequence of original Sophia.
    /// </summary>
    public bool IsSophiaSequence => Sequence == 12;

    /// <summary>
    /// Whether or not this level is in the sequence of original Willard.
    /// </summary>
    public bool IsWillardSequence => Sequence == 19;

    /// <summary>
    /// Whether or not the game will account for secrets collected in this level.
    /// </summary>
    public bool HasSecrets => Script.NumSecrets > 0;

    public bool HasVehicle => Data.Entities.Any(e => TR3TypeUtilities.IsVehicleType(e.TypeID));

    /// <summary>
    /// Get the adventure based on this level's name.
    /// </summary>
    public TR3Adventure Adventure
    {
        get
        {
            if (TR3LevelNames.SouthPacific.Contains(Name))
            {
                return TR3Adventure.SouthPacific;
            }
            else if (TR3LevelNames.London.Contains(Name))
            {
                return TR3Adventure.London;
            }
            else if (TR3LevelNames.Nevada.Contains(Name))
            {
                return TR3Adventure.Nevada;
            }
            else if (TR3LevelNames.Antarctica.Contains(Name))
            {
                return TR3Adventure.Antarctica;
            }

            return TR3Adventure.India;
        }
    }
}
