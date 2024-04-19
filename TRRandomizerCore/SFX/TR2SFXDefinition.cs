using Newtonsoft.Json;
using System.Text;
using TRLevelControl.Model;

namespace TRRandomizerCore.SFX;

public class TR2SFXDefinition
{
    public TR2SFX InternalIndex { get; set; }
    public string Description { get; set; }
    public TRSFXCreatureCategory Creature { get; set; }
    public List<TRSFXGeneralCategory> Categories { get; set; }
    public string SourceLevel { get; set; }

    [JsonIgnore]
    public TR2SoundEffect SoundEffect { get; set; }

    [JsonIgnore]
    public TRSFXGeneralCategory PrimaryCategory
    {
        get
        {
            return Categories.Count > 0 ? Categories[0] : TRSFXGeneralCategory.Misc;
        }
    }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" ID: " + InternalIndex);
        sb.Append(" Description: " + Description);
        sb.Append(" Categories: [" + string.Join(", ", Categories) + "]");
        sb.Append(" Creature: " + Creature);

        return sb.ToString();
    }
}
