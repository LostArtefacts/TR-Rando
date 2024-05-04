using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TRDataControl.Environment.Parsing;

public class EMDeserializationResolver : DefaultContractResolver
{
    protected override JsonConverter ResolveContractConverter(Type objectType)
    {
        if ((typeof(BaseEMFunction).IsAssignableFrom(objectType) || typeof(BaseEMCondition).IsAssignableFrom(objectType)) && !objectType.IsAbstract)
        {
            return null;
        }

        return base.ResolveContractConverter(objectType);
    }
}
