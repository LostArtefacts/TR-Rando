using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TREnvironmentEditor.Parsing
{
    // https://stackoverflow.com/questions/32571695/order-of-fields-when-serializing-the-derived-class-in-json-net
    public class EMSerializationResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization)
                ?.OrderBy(p => p.DeclaringType.BaseTypesAndSelf().Count()).ToList();
        }
    }

    public static class TypeExtensions
    {
        public static IEnumerable<Type> BaseTypesAndSelf(this Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }
    }
}