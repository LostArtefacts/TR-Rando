namespace TRLevelControl.Model;

public class TR1Level : TRLevelBase
{
    public List<TRTexImage8> Images8 { get; set; }
    public List<TRRoom> Rooms { get; set; }
    public List<ushort> FloorData { get; set; }
    public List<TRMesh> Meshes { get; set; }
    public List<uint> MeshPointers { get; set; }
    public List<TRMeshTreeNode> MeshTrees { get; set; }
    public List<ushort> Frames { get; set; }
    public List<TRModel> Models { get; set; }
    public List<TRStaticMesh> StaticMeshes { get; set; }
    public List<TRObjectTexture> ObjectTextures { get; set; }
    public List<TRSpriteTexture> SpriteTextures { get; set; }
    public List<TRSpriteSequence> SpriteSequences { get; set; }
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
}
