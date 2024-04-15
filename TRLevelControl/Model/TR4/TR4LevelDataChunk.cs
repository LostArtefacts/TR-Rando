using TRLevelControl.Serialization;
using TRLevelControl.Compression;

namespace TRLevelControl.Model;

public class TR4LevelDataChunk : ISerializableCompact
{
    public uint UncompressedSize { get; set; }

    public uint CompressedSize { get; set; }

    public uint Unused { get; set; }

    public ushort NumRooms { get; set; }

    public TR4Room[] Rooms { get; set; }

    public uint NumFloorData { get; set; }

    public ushort[] Floordata { get; set; }

    public uint NumMeshData { get; set; }

    public ushort[] RawMeshData { get; set; }

    public TR4Mesh[] Meshes { get; set; }

    public uint NumMeshPointers { get; set; }

    public uint[] MeshPointers { get; set; }

    public List<TR4Animation> Animations { get; set; }
    public List<TRStateChange> StateChanges { get; set; }
    public List<TRAnimDispatch> AnimDispatches { get; set; }
    public List<TRAnimCommand> AnimCommands { get; set; }
    public List<TRMeshTreeNode> MeshTrees { get; set; }
    public List<ushort> Frames { get; set; }
    public List<TRModel> Models { get; set; }

    public uint NumStaticMeshes { get; set; }

    public TRStaticMesh[] StaticMeshes { get; set; }

    public byte[] SPRMarker { get; set; }

    public uint NumSpriteTextures { get; set; }

    public TRSpriteTexture[] SpriteTextures { get; set; }

    public uint NumSpriteSequences { get; set; }

    public TRSpriteSequence[] SpriteSequences { get; set; }

    public uint NumCameras { get; set; }

    public TRCamera[] Cameras { get; set; }

    public uint NumFlybyCameras { get; set; }

    public TR4FlyByCamera[] FlybyCameras { get; set; }

    public uint NumSoundSources { get; set; }

    public TRSoundSource[] SoundSources { get; set; }

    public uint NumBoxes { get; set; }

    public TR2Box[] Boxes { get; set; }

    public uint NumOverlaps { get; set; }

    public ushort[] Overlaps { get; set; }

    public short[] Zones { get; set; }

    public uint NumAnimatedTextures { get; set; }

    public TRAnimatedTexture[] AnimatedTextures { get; set; }

    public byte AnimatedTexturesUVCount { get; set; }

    public byte[] TEXMarker { get; set; }

    public uint NumObjectTextures { get; set; }

    public TR4ObjectTexture[] ObjectTextures { get; set; }
    public List<TR4Entity> Entities { get; set; }
    public List<TR4AIEntity> AIEntities { get; set; }

    public ushort NumDemoData { get; set; }

    public byte[] DemoData { get; set; }

    public short[] SoundMap { get; set; }

    public uint NumSoundDetails { get; set; }

    public TR3SoundDetails[] SoundDetails { get; set; }

    public uint NumSampleIndices { get; set; }

    public uint[] SampleIndices { get; set; }

    public byte[] Seperator { get; set; }

    //Optional - mainly just for testing, this is just to store the raw zlib compressed chunk.
    public byte[] CompressedChunk { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (TRLevelWriter writer = new(stream))
        {
            writer.Write(Unused);
            writer.Write(NumRooms);

            foreach (TR4Room room in Rooms)
            {
                writer.Write(room.Serialize());
            }

            writer.Write(NumFloorData);

            foreach (ushort data in Floordata)
            {
                writer.Write(data);
            }

            writer.Write(NumMeshData);

            foreach (TR4Mesh mesh in Meshes)
            {
                writer.Write(mesh.Serialize());
            }

            writer.Write(NumMeshPointers);

            foreach (uint data in MeshPointers)
            {
                writer.Write(data);
            }

            writer.Write((uint)Animations.Count);
            foreach (TR4Animation anim in Animations)
            {
                writer.Write(anim.Serialize());
            }

            writer.Write((uint)StateChanges.Count);
            foreach (TRStateChange sc in StateChanges)
            {
                writer.Write(sc.Serialize());
            }

            writer.Write((uint)AnimDispatches.Count);
            foreach (TRAnimDispatch ad in AnimDispatches)
            {
                writer.Write(ad.Serialize());
            }

            writer.Write((uint)AnimCommands.Count);
            foreach (TRAnimCommand ac in AnimCommands)
            {
                writer.Write(ac.Serialize());
            }

            writer.Write((uint)MeshTrees.Count * 4); //To get the correct number /= 4 is done during read, make sure to reverse it here.
            foreach (TRMeshTreeNode node in MeshTrees)
            {
                writer.Write(node.Serialize());
            }

            writer.Write((uint)Frames.Count);
            foreach (ushort frame in Frames)
            {
                writer.Write(frame);
            }

            writer.Write((uint)Models.Count);
            foreach (TRModel model in Models)
            {
                writer.Write(model.Serialize());
            }

            writer.Write(NumStaticMeshes);

            foreach (TRStaticMesh sm in StaticMeshes)
            {
                writer.Write(sm.Serialize());
            }

            writer.Write(SPRMarker);

            writer.Write(NumSpriteTextures);

            foreach (TRSpriteTexture st in SpriteTextures)
            {
                writer.Write(st.Serialize());
            }

            writer.Write(NumSpriteSequences);

            foreach (TRSpriteSequence seq in SpriteSequences)
            {
                writer.Write(seq.Serialize());
            }

            writer.Write(NumCameras);

            foreach (TRCamera cam in Cameras)
            {
                writer.Write(cam.Serialize());
            }

            writer.Write(NumFlybyCameras);

            foreach (TR4FlyByCamera flycam in FlybyCameras)
            {
                writer.Write(flycam.Serialize());
            }

            writer.Write(NumSoundSources);

            foreach (TRSoundSource ssrc in SoundSources)
            {
                writer.Write(ssrc.Serialize());
            }

            writer.Write(NumBoxes);

            foreach (TR2Box box in Boxes)
            {
                writer.Write(box.Serialize());
            }

            writer.Write(NumOverlaps);

            foreach (ushort overlap in Overlaps)
            {
                writer.Write(overlap);
            }

            foreach (short zone in Zones)
            {
                writer.Write(zone);
            }

            writer.Write(NumAnimatedTextures);
            writer.Write((ushort)AnimatedTextures.Length);
            foreach (TRAnimatedTexture texture in AnimatedTextures) { writer.Write(texture.Serialize()); }
            writer.Write(AnimatedTexturesUVCount);

            writer.Write(TEXMarker);

            writer.Write(NumObjectTextures);

            foreach (TR4ObjectTexture otex in ObjectTextures)
            {
                writer.Write(otex.Serialize());
            }

            writer.Write((uint)Entities.Count);
            writer.Write(Entities);

            writer.Write((uint)AIEntities.Count);
            writer.Write(AIEntities);

            writer.Write(NumDemoData);
            writer.Write(DemoData);

            foreach (short sound in SoundMap)
            {
                writer.Write(sound);
            }

            writer.Write(NumSoundDetails);

            foreach (TR3SoundDetails snd in SoundDetails)
            {
                writer.Write(snd.Serialize());
            }

            writer.Write(NumSampleIndices);

            foreach (uint sampleindex in SampleIndices)
            {
                writer.Write(sampleindex);
            }

            writer.Write(Seperator);
        }

        byte[] uncompressed = stream.ToArray();
        UncompressedSize = (uint)uncompressed.Length;

        byte[] compressed = TRZlib.Compress(uncompressed);
        CompressedSize = (uint)compressed.Length;

        return compressed;
    }
}
