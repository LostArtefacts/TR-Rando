using Newtonsoft.Json;
using TRLevelControl.Model;

namespace TRRandomizerCore.SFX;

public class TRSFXDefinition<T, E>
    where T : Enum
    where E : class
{
    public T InternalIndex { get; set; }
    public string Description { get; set; }
    public TRSFXCreatureCategory Creature { get; set; }
    public List<TRSFXGeneralCategory> Categories { get; set; }
    public string SourceLevel { get; set; }
    public uint? GoldSampleRemap { get; set; }

    [JsonIgnore]
    public E SoundEffect { get; set; }

    [JsonIgnore]
    public TRSFXGeneralCategory PrimaryCategory
    {
        get
        {
            return Categories.Count > 0 ? Categories[0] : TRSFXGeneralCategory.Misc;
        }
    }
}

public class TR1SFXDefinition : TRSFXDefinition<TR1SFX, TR1SoundEffect> { }
public class TR2SFXDefinition : TRSFXDefinition<TR2SFX, TR2SoundEffect> { }
public class TR3SFXDefinition : TRSFXDefinition<TR3SFX, TR3SoundEffect> { }
