using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Model.Types;

namespace TREnvironmentEditor.Parsing
{
    public class EMConverter : JsonConverter
    {
        private static readonly JsonSerializerSettings _resolver = new JsonSerializerSettings
        { 
            ContractResolver = new EMResolver() 
        };

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BaseEMFunction);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            if (jo["EMType"] == null)
            {
                return null;
            }

            EMType type = (EMType)jo["EMType"].Value<int>();
            switch (type)
            {
                case EMType.Floor:
                    return JsonConvert.DeserializeObject<EMFloorFunction>(jo.ToString(), _resolver);
                case EMType.Ladder:
                    return JsonConvert.DeserializeObject<EMLadderFunction>(jo.ToString(), _resolver);
                case EMType.Flood:
                    return JsonConvert.DeserializeObject<EMFloodFunction>(jo.ToString(), _resolver);
                case EMType.Drain:
                    return JsonConvert.DeserializeObject<EMDrainFunction>(jo.ToString(), _resolver);
                case EMType.Trigger:
                    return JsonConvert.DeserializeObject<EMTriggerFunction>(jo.ToString(), _resolver);
                case EMType.DeleteTrigger:
                    return JsonConvert.DeserializeObject<EMDeleteTriggerFunction>(jo.ToString(), _resolver);
                default:
                    throw new InvalidOperationException();
            }
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
    }
}