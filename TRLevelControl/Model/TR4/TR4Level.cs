namespace TRLevelControl.Model;

public class TR4Level : TRLevelBase
{
    public TR4Texture32Chunk Texture32Chunk { get; set; }
    public TR4Texture16Chunk Texture16Chunk { get; set; }
    public TR4SkyAndFont32Chunk SkyAndFont32Chunk { get; set; }

    public List<TR4Room> Rooms { get; set; }
    public List<ushort> FloorData { get; set; }
    public List<TR4Mesh> Meshes { get; set; }
    public List<uint> MeshPointers { get; set; }
    public List<TR4Animation> Animations { get; set; }
    public List<TRStateChange> StateChanges { get; set; }
    public List<TRAnimDispatch> AnimDispatches { get; set; }
    public List<TRAnimCommand> AnimCommands { get; set; }
    public List<TRMeshTreeNode> MeshTrees { get; set; }
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
    public List<TR4ObjectTexture> ObjectTextures { get; set; }
    public List<TR4Entity> Entities { get; set; }
    public List<TR4AIEntity> AIEntities { get; set; }
    public byte[] DemoData { get; set; }
    public short[] SoundMap { get; set; }
    public List<TR3SoundDetails> SoundDetails { get; set; }
    public List<uint> SampleIndices { get; set; }

    public List<TR4Sample> Samples { get; set; }
}
