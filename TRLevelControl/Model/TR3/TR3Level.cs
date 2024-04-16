namespace TRLevelControl.Model;

public class TR3Level : TRLevelBase
{
    public List<TRColour> Palette { get; set; }
    public List<TRColour4> Palette16 { get; set; }
    public List<TRTexImage8> Images8 { get; set; }
    public List<TRTexImage16> Images16 { get; set; }
    public List<TR3Room> Rooms { get; set; }
    public List<ushort> FloorData { get; set; }
    public List<TRMesh> Meshes { get; set; }
    public List<uint> MeshPointers { get; set; }
    public List<TRAnimation> Animations { get; set; }
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
    public List<TRSoundSource> SoundSources { get; set; }
    public List<TR2Box> Boxes { get; set; }
    public List<ushort> Overlaps { get; set; }
    public List<TR2ZoneGroup> Zones { get; set; }
    public List<TRAnimatedTexture> AnimatedTextures { get; set; }
    public List<TRObjectTexture> ObjectTextures { get; set; }
    public List<TR3Entity> Entities { get; set; }
    public List<byte> LightMap { get; set; }
    public List<TRCinematicFrame> CinematicFrames { get; set; }

    /// <summary>
    /// 2 bytes
    /// </summary>
    public ushort NumDemoData { get; set; }

    /// <summary>
    /// NumDemoData bytes
    /// </summary>
    public byte[] DemoData { get; set; }

    /// <summary>
    /// 370 entries of 2 bytes each = 740 bytes
    /// </summary>
    public short[] SoundMap { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumSoundDetails { get; set; }

    /// <summary>
    /// NumSoundDetails * 8 bytes
    /// </summary>
    public TR3SoundDetails[] SoundDetails { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumSampleIndices { get; set; }

    /// <summary>
    /// NumSampleIndices * 4 bytes
    /// </summary>
    public uint[] SampleIndices { get; set; }
}
