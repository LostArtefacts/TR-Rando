using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using TREnvironmentEditor.Model;

namespace TREnvironmentEditor.Parsing
{
    public class EMResolver : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (typeof(BaseEMFunction).IsAssignableFrom(objectType) && !objectType.IsAbstract)
            {
                return null;
            }

            return base.ResolveContractConverter(objectType);
        }
    }
}