namespace TRLevelControl.Model;

public class TR1Level : TRLevelBase
{
    public List<TRTexImage8> Images8 { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint Unused { get; set; }

    /// <summary>
    /// 2 bytes
    /// </summary>
    public ushort NumRooms { get; set; }

    /// <summary>
    /// Variable
    /// </summary>
    public TRRoom[] Rooms { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumFloorData { get; set; }

    /// <summary>
    /// NumFloorData * 2 bytes
    /// </summary>
    public ushort[] FloorData { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumMeshData { get; set; }

    /// <summary>
    /// 2 * NumMeshData, holds the raw data stored in Meshes
    /// </summary>
    public ushort[] RawMeshData { get; set; }

    /// <summary>
    /// Variable
    /// </summary>
    public TRMesh[] Meshes { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumMeshPointers { get; set; }

    /// <summary>
    /// NumMeshPointers * 4 bytes
    /// </summary>
    public uint[] MeshPointers { get; set; }

    public List<TRAnimation> Animations { get; set; }
    public List<TRStateChange> StateChanges { get; set; }
    public List<TRAnimDispatch> AnimDispatches { get; set; }
    public List<TRAnimCommand> AnimCommands { get; set; }
    public List<TRMeshTreeNode> MeshTrees { get; set; }
    public List<ushort> Frames { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumModels { get; set; }

    /// <summary>
    /// NumModels * 18 bytes
    /// </summary>
    public TRModel[] Models { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumStaticMeshes { get; set; }

    /// <summary>
    /// NumStaticMeshes * 32 bytes
    /// </summary>
    public TRStaticMesh[] StaticMeshes { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumObjectTextures { get; set; }

    /// <summary>
    /// NumObjectTextures * 20 bytes
    /// </summary>
    public TRObjectTexture[] ObjectTextures { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumSpriteTextures { get; set; }

    /// <summary>
    /// NumSpriteTextures * 16 bytes
    /// </summary>
    public TRSpriteTexture[] SpriteTextures { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumSpriteSequences { get; set; }

    /// <summary>
    /// NumSpriteSequences * 8 bytes
    /// </summary>
    public TRSpriteSequence[] SpriteSequences { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumCameras { get; set; }

    /// <summary>
    /// NumCameras * 16 bytes
    /// </summary>
    public TRCamera[] Cameras { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumSoundSources { get; set; }

    /// <summary>
    /// NumSoundSources * 16 bytes
    /// </summary>
    public TRSoundSource[] SoundSources { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumBoxes { get; set; }

    /// <summary>
    /// NumBoxes * 8 bytes
    /// </summary>
    public TRBox[] Boxes { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumOverlaps { get; set; }

    /// <summary>
    /// NumOverlaps * 2 bytes
    /// </summary>
    public ushort[] Overlaps { get; set; }

    public TRZoneGroup[] Zones { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumAnimatedTextures { get; set; }

    /// <summary>
    /// NumAnimatesTextures * 2 bytes
    /// </summary>
    public TRAnimatedTexture[] AnimatedTextures { get; set; }
    public List<TR1Entity> Entities { get; set; }
    public List<byte> LightMap { get; set; }
    public List<TRColour> Palette { get; set; }

    /// <summary>
    /// 2 bytes
    /// </summary>
    public ushort NumCinematicFrames { get; set; }

    /// <summary>
    /// NumCinematicFrames * 16 bytes
    /// </summary>
    public TRCinematicFrame[] CinematicFrames { get; set; }

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
    public TRSoundDetails[] SoundDetails { get; set; }

    public uint NumSamples { get; set; }

    public byte[] Samples { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumSampleIndices { get; set; }

    /// <summary>
    /// NumSampleIndices * 4 bytes
    /// </summary>
    public uint[] SampleIndices { get; set; }
}
