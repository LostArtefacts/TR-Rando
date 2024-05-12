using Newtonsoft.Json;
using TRDataControl.Environment.Parsing;

namespace TRDataControl.Environment;

public class EMEditorMapping
{
    public static readonly EMConverter Converter = new();
    public static readonly JsonSerializerSettings Serializer = new()
    {
        ContractResolver = new EMSerializationResolver(),
        DefaultValueHandling = DefaultValueHandling.Ignore,
        Formatting = Formatting.Indented
    };

    public EMEditorSet All { get; set; } = new();
    public List<EMEditorSet> Any { get; set; } = new();
    public List<List<EMEditorSet>> AllWithin { get; set; } = new();
    public List<EMEditorGroupedSet> OneOf { get; set; } = new();
    public List<EMConditionalEditorSet> ConditionalAllWithin { get; set; } = new();
    public List<EMConditionalSingleEditorSet> ConditionalAll { get; set; } = new();
    public List<EMConditionalGroupedSet> ConditionalOneOf { get; set; } = new();
    public EMEditorSet Mirrored { get; set; } = new();
    public Dictionary<ushort, ushort> AlternativeTextures { get; set; }

    public static EMEditorMapping Get(string packPath)
    {
        if (File.Exists(packPath))
        {
            return JsonConvert.DeserializeObject<EMEditorMapping>(File.ReadAllText(packPath), Converter);
        }

        return null;
    }

    public void SerializeTo(string packPath)
    {
        File.WriteAllText(packPath, Serialize());
    }

    public string Serialize()
    {
        return JsonConvert.SerializeObject(this, Serializer);
    }

    public void AlternateTextures()
    {
        if (AlternativeTextures == null)
        {
            return;
        }

        All?.RemapTextures(AlternativeTextures);
        ConditionalAll?.ForEach(s => s.RemapTextures(AlternativeTextures));
        Any?.ForEach(s => s.RemapTextures(AlternativeTextures));
        AllWithin?.ForEach(a => a.ForEach(s => s.RemapTextures(AlternativeTextures)));
        ConditionalAllWithin?.ForEach(s => s.RemapTextures(AlternativeTextures));
        OneOf?.ForEach(s => s.RemapTextures(AlternativeTextures));
        ConditionalOneOf?.ForEach(s => s.RemapTextures(AlternativeTextures));
        Mirrored?.RemapTextures(AlternativeTextures);
    }

    public void SetCommunityPatch(bool isCommunityPatch)
    {
        All?.SetCommunityPatch(isCommunityPatch);
        ConditionalAll?.ForEach(s => s.SetCommunityPatch(isCommunityPatch));
        Any?.ForEach(s => s.SetCommunityPatch(isCommunityPatch));
        AllWithin?.ForEach(a => a.ForEach(s => s.SetCommunityPatch(isCommunityPatch)));
        ConditionalAllWithin?.ForEach(s => s.SetCommunityPatch(isCommunityPatch));
        OneOf?.ForEach(s => s.SetCommunityPatch(isCommunityPatch));
        ConditionalOneOf?.ForEach(s => s.SetCommunityPatch(isCommunityPatch));
        Mirrored?.SetCommunityPatch(isCommunityPatch);
    }

    public List<BaseEMFunction> FindAll(Predicate<BaseEMFunction> predicate = null)
    {
        List<BaseEMFunction> results = new();
        Scan(e =>
        {
            if (predicate == null || predicate(e))
            {
                results.Add(e);
            }
        });
        return results;
    }

    public void Scan(Action<BaseEMFunction> callback)
    {
        All?.ForEach(e => callback(e));
        ConditionalAll?.ForEach(s => s.OnFalse?.ForEach(e => callback(e)));
        ConditionalAll?.ForEach(s => s.OnTrue?.ForEach(e => callback(e)));
        Any?.ForEach(e => e.ForEach(a => callback(a)));
        AllWithin?.ForEach(a => a.ForEach(s => s.ForEach(e => callback(e))));
        ConditionalAllWithin?.ForEach(s => s.OnFalse?.ForEach(a => a.ForEach(e => callback(e))));
        ConditionalAllWithin?.ForEach(s => s.OnTrue?.ForEach(a => a.ForEach(e => callback(e))));
        OneOf?.ForEach(s => s.Leader.ForEach(e => callback(e)));
        OneOf?.ForEach(s => s.Followers.ForEach(e => e.ForEach(a => callback(a))));
        ConditionalOneOf?.ForEach(s => s.OnFalse?.Leader.ForEach(e => callback(e)));
        ConditionalOneOf?.ForEach(s => s.OnFalse?.Followers.ForEach(e => e.ForEach(a => callback(a))));
        ConditionalOneOf?.ForEach(s => s.OnTrue?.Leader.ForEach(e => callback(e)));
        ConditionalOneOf?.ForEach(s => s.OnTrue?.Followers.ForEach(e => e.ForEach(a => callback(a))));
        Mirrored?.ForEach(e => callback(e));
    }
}
