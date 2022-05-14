using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using TREnvironmentEditor.Model;

namespace TREnvironmentEditor.Parsing
{
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
}