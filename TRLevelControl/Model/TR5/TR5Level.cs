namespace TRLevelControl.Model;

public class TR5Level : TRLevelBase
{
    public TR5Textiles Images { get; set; }
    public ushort LaraType { get; set; }
    public ushort WeatherType { get; set; }
    public List<TR5Room> Rooms { get; set; }
    public List<ushort> FloorData { get; set; }
    public List<TRMesh> Meshes { get; set; }
    public List<uint> MeshPointers { get; set; }
    public List<ushort> Frames { get; set; }
    public List<TRModel> Models { get; set; }
    public List<TRStaticMesh> StaticMeshes { get; set; }
    public List<TRSpriteTexture> SpriteTextures { get; set; }
    public List<TRSpriteSequence> SpriteSequences { get; set; }
    public List<TRCamera> Cameras { get; set; }
    public List<TR4FlyByCamera> FlybyCameras { get; set; }
    public List<TRSoundSource> SoundSources { get; set; }
    public List<TR2Box> Boxes { get; set; }
    public List<ushort> Overlaps { get; set; }
    public List<short> Zones { get; set; }
    public List<TRAnimatedTexture> AnimatedTextures { get; set; }
    public byte AnimatedTexturesUVCount { get; set; }
    public List<TR5ObjectTexture> ObjectTextures { get; set; }
    public List<TR5Entity> Entities { get; set; }
    public List<TR5AIEntity> AIEntities { get; set; }
    public byte[] DemoData { get; set; }
    public SortedDictionary<TR5SFX, TR4SoundEffect> SoundEffects { get; set; }
}
