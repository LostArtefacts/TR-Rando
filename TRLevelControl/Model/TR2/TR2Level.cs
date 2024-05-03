namespace TRLevelControl.Model;

public class TR2Level : TRLevelBase
{
    public List<TRColour> Palette { get; set; }
    public List<TRColour4> Palette16 { get; set; }
    public List<TRTexImage8> Images8 { get; set; }
    public List<TRTexImage16> Images16 { get; set; }
    public List<TR2Room> Rooms { get; set; }
    public TRDictionary<TR2Type, TRModel> Models { get; set; }
    public TRDictionary<TR2Type, TRStaticMesh> StaticMeshes { get; set; }
    public List<TRObjectTexture> ObjectTextures { get; set; }
    public TRDictionary<TR2Type, TRSpriteSequence> Sprites { get; set; }
    public List<TRSoundSource<TR2SFX>> SoundSources { get; set; }
    public List<TRAnimatedTexture> AnimatedTextures { get; set; }
    public List<TR2Entity> Entities { get; set; }
    public List<byte> LightMap { get; set; }
    public List<TRCinematicFrame> CinematicFrames { get; set; }
    public byte[] DemoData { get; set; }
    public SortedDictionary<TR2SFX, TR2SoundEffect> SoundEffects { get; set; }

    public override IEnumerable<TRMesh> DistinctMeshes => Models.Values.SelectMany(m => m.Meshes)
        .Concat(StaticMeshes.Values.Select(s => s.Mesh))
        .Distinct();
}
