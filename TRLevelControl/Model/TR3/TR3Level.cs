namespace TRLevelControl.Model;

public class TR3Level : TRLevelBase
{
    /// <summary>
    /// 256 entries * 3 components = 768 Bytes
    /// </summary>
    public TRColour[] Palette { get; set; }

    /// <summary>
    /// 256 entries * 4 components = 1024 bytes
    /// </summary>
    public TRColour4[] Palette16 { get; set; }

    /// <summary>
    /// 4 Bytes
    /// </summary>
    public uint NumImages { get; set; }

    /// <summary>
    /// NumImages * 65536 bytes
    /// </summary>
    public TRTexImage8[] Images8 { get; set; }

    /// <summary>
    /// NumImages * 131072 bytes
    /// </summary>
    public TRTexImage16[] Images16 { get; set; }

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
    public TR3Room[] Rooms { get; set; }

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

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumAnimations { get; set; }

    /// <summary>
    /// NumAnimations * 32 bytes
    /// </summary>
    public TRAnimation[] Animations { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumStateChanges { get; set; }

    /// <summary>
    /// NumStateChanges * 6 bytes
    /// </summary>
    public TRStateChange[] StateChanges { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumAnimDispatches { get; set; }

    /// <summary>
    /// NumAnimDispatches * 8 bytes
    /// </summary>
    public TRAnimDispatch[] AnimDispatches { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumAnimCommands { get; set; }

    /// <summary>
    /// NumAnimCommands * 2 bytes
    /// </summary>
    public TRAnimCommand[] AnimCommands { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumMeshTrees { get; set; }

    /// <summary>
    /// NumMeshTrees * 4 bytes
    /// </summary>
    public TRMeshTreeNode[] MeshTrees { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumFrames { get; set; }

    /// <summary>
    /// NumFrames * 2 bytes
    /// </summary>
    public ushort[] Frames { get; set; }

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
    public TR2Box[] Boxes { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumOverlaps { get; set; }

    /// <summary>
    /// NumOverlaps * 2 bytes
    /// </summary>
    public ushort[] Overlaps { get; set; }

    /// <summary>
    /// NumBoxes * 20 bytes (double check this)
    /// </summary>
    public TR2ZoneGroup[] Zones { get; set; }

    /// <summary>
    /// 4 bytes
    /// </summary>
    public uint NumAnimatedTextures { get; set; }

    /// <summary>
    /// NumAnimatesTextures * 2 bytes
    /// </summary>
    public TRAnimatedTexture[] AnimatedTextures { get; set; }

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
    public uint NumEntities { get; set; }

    /// <summary>
    /// NumEntities * 24 bytes
    /// </summary>
    public TR2Entity[] Entities { get; set; }

    /// <summary>
    /// (32 * 256 entries) of 1 byte = 8192 bytes
    /// </summary>
    public byte[] LightMap { get; set; }

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
