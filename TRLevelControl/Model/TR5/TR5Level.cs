namespace TRLevelControl.Model;

public class TR5Level : TRLevelBase
{
    public TR5Textiles Images { get; set; }
    public ushort LaraType { get; set; }
    public ushort WeatherType { get; set; }
    public List<TR5Room> Rooms { get; set; }
    public TRDictionary<TR5Type, TRModel> Models { get; set; }
    public TRDictionary<TR5Type, TRStaticMesh> StaticMeshes { get; set; }
    public TRDictionary<TR5Type, TRSpriteSequence> Sprites { get; set; }
    public TRDictionary<byte, List<TRFlybyCamera>> Flybys { get; set; }
    public List<TRSoundSource<TR5SFX>> SoundSources { get; set; }
    public List<TRAnimatedTexture> AnimatedTextures { get; set; }
    public byte AnimatedTexturesUVCount { get; set; }
    public List<TR5ObjectTexture> ObjectTextures { get; set; }
    public List<TR5Entity> Entities { get; set; }
    public List<TR5AIEntity> AIEntities { get; set; }
    public byte[] DemoData { get; set; }
    public SortedDictionary<TR5SFX, TR4SoundEffect> SoundEffects { get; set; }

    public override IEnumerable<TRMesh> DistinctMeshes => Models.Values.SelectMany(m => m.Meshes)
        .Concat(StaticMeshes.Values.Select(s => s.Mesh))
        .Distinct();
}
