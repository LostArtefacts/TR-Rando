using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Reflection;
using TRLevelControl.Model;

namespace TRDataControl;

public class TRBlobResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        // Don't output dynamic getters
        IEnumerable<PropertyInfo> props = type.GetProperties()
            .Where(p => p.CanWrite || type.BaseType == typeof(TRAnimCommand));

        return base.CreateProperties(type, memberSerialization)
            ?.Where(p => !p.Ignored && props.Any(pi => pi.Name == p.PropertyName))
            .ToList();
    }
}
