using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TR5LevelDataChunk
    {
        public uint UncompressedSize { get; set; }

        public uint CompressedSize { get; set; }

        public uint Unused { get; set; }

        public ushort NumRooms { get; set; }

        public TR5Room[] Rooms { get; set; }

        public uint NumFloorData { get; set; }

        public ushort[] Floordata { get; set; }

        public uint NumMeshData { get; set; }

        public ushort[] RawMeshData { get; set; }

        public TR4Mesh[] Meshes { get; set; }

        public uint NumMeshPointers { get; set; }

        public uint[] MeshPointers { get; set; }

        public uint NumAnimations { get; set; }

        public TR4Animation[] Animations { get; set; }

        public uint NumStateChanges { get; set; }

        public TRStateChange[] StateChanges { get; set; }

        public uint NumAnimDispatches { get; set; }

        public TRAnimDispatch[] AnimDispatches { get; set; }

        public uint NumAnimCommands { get; set; }

        public TRAnimCommand[] AnimCommands { get; set; }

        public uint NumMeshTrees { get; set; }

        public TRMeshTreeNode[] MeshTrees { get; set; }

        public uint NumFrames { get; set; }

        public ushort[] Frames { get; set; }

        public uint NumModels { get; set; }

        public TRModel[] Models { get; set; }

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

        public uint NumEntities { get; set; }

        public TR4Entity[] Entities { get; set; }

        public uint NumAIObjects { get; set; }

        public TR4AIObject[] AIObjects { get; set; }

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
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Unused);
                    writer.Write(NumRooms);

                    foreach (TR5Room room in Rooms)
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

                    writer.Write(NumAnimations);

                    foreach (TR4Animation anim in Animations)
                    {
                        writer.Write(anim.Serialize());
                    }

                    writer.Write(NumStateChanges);

                    foreach (TRStateChange sc in StateChanges)
                    {
                        writer.Write(sc.Serialize());
                    }

                    writer.Write(NumAnimDispatches);

                    foreach (TRAnimDispatch ad in AnimDispatches)
                    {
                        writer.Write(ad.Serialize());
                    }

                    writer.Write(NumAnimCommands);

                    foreach (TRAnimCommand ac in AnimCommands)
                    {
                        writer.Write(ac.Serialize());
                    }

                    writer.Write(NumMeshTrees * 4); //To get the correct number /= 4 is done during read, make sure to reverse it here.

                    foreach (TRMeshTreeNode node in MeshTrees)
                    {
                        writer.Write(node.Serialize());
                    }

                    writer.Write(NumFrames);

                    foreach (ushort frame in Frames)
                    {
                        writer.Write(frame);
                    }

                    writer.Write(NumModels);

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

                    writer.Write(NumEntities);

                    foreach (TR4Entity ent in Entities)
                    {
                        writer.Write(ent.Serialize());
                    }

                    writer.Write(NumAIObjects);

                    foreach (TR4AIObject ai in AIObjects)
                    {
                        writer.Write(ai.Serialize());
                    }

                    writer.Write(NumDemoData);
                    writer.Write(DemoData);

                    foreach (ushort sound in SoundMap)
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
                this.UncompressedSize = (uint)uncompressed.Length;
                this.CompressedSize = this.UncompressedSize;

                return uncompressed;
            }
        }
    }
}
