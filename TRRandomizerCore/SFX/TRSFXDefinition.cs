using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace TRRandomizerCore.SFX
{
    public class TRSFXDefinition<D> where D : class
    {
        public short InternalIndex { get; set; }
        public string Description { get; set; }
        public TRSFXCreatureCategory Creature { get; set; }
        public List<TRSFXGeneralCategory> Categories { get; set; }
        public D Details { get; set; }
        public List<uint> SampleIndices { get; set; }

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
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" ID: " + InternalIndex);
            sb.Append(" Description: " + Description);
            sb.Append(" Categories: [" + string.Join(", ", Categories) + "]");
            sb.Append(" Creature: " + Creature);

            return sb.ToString();
        }
    }
}