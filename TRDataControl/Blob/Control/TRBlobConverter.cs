using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TRLevelControl.Model;

namespace TRDataControl;

public class TRBlobConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TRAnimCommand) || objectType == typeof(TRTexture);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        if (objectType == typeof(TRAnimCommand))
        {
            return ReadAnimCommand(jo);
        }
        if (objectType == typeof(TRTexture))
        {
            return ReadTexture(jo);
        }
        return existingValue;
    }

    private static object ReadAnimCommand(JObject jo)
    {
        TRAnimCommandType type = (TRAnimCommandType)jo[nameof(TRAnimCommand.Type)].Value<int>();
        return type switch
        {
            TRAnimCommandType.EmptyHands => JsonConvert.DeserializeObject<TREmptyHandsCommand>(jo.ToString()),
            TRAnimCommandType.Null => JsonConvert.DeserializeObject<TRNullCommand>(jo.ToString()),
            TRAnimCommandType.SetPosition => JsonConvert.DeserializeObject<TRSetPositionCommand>(jo.ToString()),
            TRAnimCommandType.JumpDistance => JsonConvert.DeserializeObject<TRJumpDistanceCommand>(jo.ToString()),
            TRAnimCommandType.Kill => JsonConvert.DeserializeObject<TRKillCommand>(jo.ToString()),
            TRAnimCommandType.PlaySound => JsonConvert.DeserializeObject<TRSFXCommand>(jo.ToString()),
            TRAnimCommandType.FlipEffect =>
                jo[nameof(TRFootprintCommand.Foot)] == null
                ? JsonConvert.DeserializeObject<TRFXCommand>(jo.ToString())
                : JsonConvert.DeserializeObject<TRFootprintCommand>(jo.ToString()),
            _ => throw new InvalidDataException(),
        };
    }

    private static TRTexture ReadTexture(JObject jo)
    {
        return jo[nameof(TRObjectTexture.Vertices)] == null
            ? JsonConvert.DeserializeObject<TRSpriteTexture>(jo.ToString())
            : JsonConvert.DeserializeObject<TRObjectTexture>(jo.ToString());
    }

    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { }
}
