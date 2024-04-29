namespace TRLevelControl.Model;

public class TR1Level : TRLevelBase
{
    public List<TRTexImage8> Images8 { get; set; }
    public List<TR1Room> Rooms { get; set; }
    public List<ushort> FloorData { get; set; }
    public TRDictionary<TR1Type, TRModel> Models { get; set; }
    public TRDictionary<TR1Type, TRStaticMesh> StaticMeshes { get; set; }
    public List<TRObjectTexture> ObjectTextures { get; set; }
    public TRDictionary<TR1Type, TRSpriteSequence> Sprites { get; set; }
    public List<TRCamera> Cameras { get; set; }
    public List<TRSoundSource> SoundSources { get; set; }
    public List<TRBox> Boxes { get; set; }
    public List<ushort> Overlaps { get; set; }
    public List<TRZoneGroup> Zones { get; set; }
    public List<TRAnimatedTexture> AnimatedTextures { get; set; }
    public List<TR1Entity> Entities { get; set; }
    public List<byte> LightMap { get; set; }
    public List<TRColour> Palette { get; set; }
    public List<TRCinematicFrame> CinematicFrames { get; set; }
    public byte[] DemoData { get; set; }
    public SortedDictionary<TR1SFX, TR1SoundEffect> SoundEffects { get; set; }

    public override IEnumerable<TRMesh> DistinctMeshes => Models.Values.SelectMany(m => m.Meshes)
        .Concat(StaticMeshes.Values.Select(s => s.Mesh))
        .Distinct();
}
