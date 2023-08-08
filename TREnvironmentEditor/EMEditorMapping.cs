using Newtonsoft.Json;
using TREnvironmentEditor.Model;
using TREnvironmentEditor.Parsing;

namespace TREnvironmentEditor;

public class EMEditorMapping
{
    public static readonly EMConverter Converter = new();
    public static readonly JsonSerializerSettings Serializer = new()
    {
        ContractResolver = new EMSerializationResolver(),
        DefaultValueHandling = DefaultValueHandling.Ignore,
        Formatting = Formatting.Indented
    };

    public EMEditorSet All { get; set; }
    public EMEditorSet NonPurist { get; set; }
    public List<EMEditorSet> Any { get; set; }
    public List<List<EMEditorSet>> AllWithin { get; set; }
    public List<EMEditorGroupedSet> OneOf { get; set; }
    public List<EMConditionalEditorSet> ConditionalAllWithin { get; set; }
    public List<EMConditionalSingleEditorSet> ConditionalAll { get; set; }
    public List<EMConditionalGroupedSet> ConditionalOneOf { get; set; }
    public EMEditorSet Mirrored { get; set; }
    public Dictionary<ushort, ushort> AlternativeTextures { get; set; }

    public EMEditorMapping()
    {
        All = new EMEditorSet();
        ConditionalAll = new List<EMConditionalSingleEditorSet>();
        NonPurist = new EMEditorSet();
        Any = new List<EMEditorSet>();
        AllWithin = new List<List<EMEditorSet>>();
        ConditionalAllWithin = new List<EMConditionalEditorSet>();
        OneOf = new List<EMEditorGroupedSet>();
        ConditionalOneOf = new List<EMConditionalGroupedSet>();
        Mirrored = new EMEditorSet();
    }

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
        NonPurist?.RemapTextures(AlternativeTextures);
        Any?.ForEach(s => s.RemapTextures(AlternativeTextures));
        AllWithin?.ForEach(a => a.ForEach(s => s.RemapTextures(AlternativeTextures)));
        ConditionalAllWithin?.ForEach(s => s.RemapTextures(AlternativeTextures));
        OneOf?.ForEach(s => s.RemapTextures(AlternativeTextures));
        ConditionalOneOf?.ForEach(s => s.RemapTextures(AlternativeTextures));
        Mirrored?.RemapTextures(AlternativeTextures);
    }
}
