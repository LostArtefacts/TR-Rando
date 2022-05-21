using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using TREnvironmentEditor;

namespace TRRandomizerCore.Secrets
{
    public class TRSecretMapping<E> where E : class
    {
        public List<int> RewardEntities { get; set; }
        public List<int> JPRewardEntities { get; set; }
        public List<TRSecretRoom<E>> Rooms { get; set; }

        public static TRSecretMapping<E> Get(string packPath)
        {
            if (File.Exists(packPath))
            {
                return JsonConvert.DeserializeObject<TRSecretMapping<E>>(File.ReadAllText(packPath), EMEditorMapping.Converter);
            }

            return null;
        }

        public void SerializeTo(string packPath)
        {
            File.WriteAllText(packPath, Serialize());
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, EMEditorMapping.Serializer);
        }
    }
}