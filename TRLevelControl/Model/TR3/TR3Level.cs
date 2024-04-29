namespace TRLevelControl.Model;

public class TR3Level : TRLevelBase
{
    public List<TRColour> Palette { get; set; }
    public List<TRColour4> Palette16 { get; set; }
    public List<TRTexImage8> Images8 { get; set; }
    public List<TRTexImage16> Images16 { get; set; }
    public List<TR3Room> Rooms { get; set; }
    public List<ushort> FloorData { get; set; }
    public TRDictionary<TR3Type, TRModel> Models { get; set; }
    public List<TRStaticMesh> StaticMeshes { get; set; }
    public TRDictionary<TR3Type, TRSpriteSequence> Sprites { get; set; }
    public List<TRCamera> Cameras { get; set; }
    public List<TRSoundSource> SoundSources { get; set; }
    public List<TR2Box> Boxes { get; set; }
    public List<ushort> Overlaps { get; set; }
    public List<TR2ZoneGroup> Zones { get; set; }
    public List<TRAnimatedTexture> AnimatedTextures { get; set; }
    public List<TRObjectTexture> ObjectTextures { get; set; }
    public List<TR3Entity> Entities { get; set; }
    public List<byte> LightMap { get; set; }
    public List<TRCinematicFrame> CinematicFrames { get; set; }
    public byte[] DemoData { get; set; }
    public SortedDictionary<TR3SFX, TR3SoundEffect> SoundEffects { get; set; }

    public override IEnumerable<TRMesh> DistinctMeshes => Models.Values.SelectMany(m => m.Meshes)
        .Concat(StaticMeshes.Select(s => s.Mesh))
        .Distinct();
}
