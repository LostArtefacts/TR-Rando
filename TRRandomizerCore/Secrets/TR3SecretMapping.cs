using Newtonsoft.Json;
using TREnvironmentEditor;
using TRLevelControl.Model;

namespace TRRandomizerCore.Secrets;

public class TR3SecretMapping : TRSecretMapping<TR3Entity>
{
    public static TR3SecretMapping Get(string packPath, bool japaneseVersion)
    {
        if (!File.Exists(packPath))
        {
            return null;
        }

        TR3SecretMapping mapping = JsonConvert.DeserializeObject<TR3SecretMapping>(File.ReadAllText(packPath), EMEditorMapping.Converter);
        if (japaneseVersion && mapping.JPRewardEntities != null)
        {
            mapping.RewardEntities = mapping.JPRewardEntities;
        }
        return mapping;
    }
}
