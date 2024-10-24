using Newtonsoft.Json;
using TRDataControl.Environment;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Secrets;

public class TR3SecretMapping : TRSecretMapping<TR3Entity>
{
    public static TR3SecretMapping Get(TR3CombinedLevel level)
    {
        string packPath = $"Resources/TR3/SecretMapping/{level.Name}-SecretMapping.json";
        if (!File.Exists(packPath))
        {
            return null;
        }

        TR3SecretMapping mapping = JsonConvert.DeserializeObject<TR3SecretMapping>(File.ReadAllText(packPath), EMEditorMapping.Converter);
        if (level.Is(TR3LevelNames.THAMES)
            && level.Data.Entities.Any(e => e.TypeID == TR3Type.TeethSpikesOrBarbedWire && e.Room == 8)
            && mapping.JPRewardEntities != null)
        {
            mapping.RewardEntities = mapping.JPRewardEntities;
        }
        return mapping;
    }
}
