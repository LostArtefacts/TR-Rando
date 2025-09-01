using Newtonsoft.Json;

namespace TRRandomizerCore.Helpers;

public static class JsonUtils
{
    public static T ReadFile<T>(string filePath)
        => JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
}
