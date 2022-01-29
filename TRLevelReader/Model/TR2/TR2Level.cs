using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Helpers;
using TRLevelReader.Serialization;

//https://trwiki.earvillage.net/doku.php?id=trsone

namespace TRLevelReader.Model
{
    public class TR2Level : BaseTRLevel, ISerializableCompact
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
        public TR2Room[] Rooms { get; set; }

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
        public TRSoundDetails[] SoundDetails { get; set; }

        /// <summary>
        /// 4 bytes
        /// </summary>
        public uint NumSampleIndices { get; set; }

        /// <summary>
        /// NumSampleIndices * 4 bytes
        /// </summary>
        public uint[] SampleIndices { get; set; }

        public TR2Level()
        {

        }

        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(Version);
                    foreach (TRColour col in Palette) { writer.Write(col.Serialize()); }
                    foreach (TRColour4 col in Palette16) { writer.Write(col.Serialize()); }
                    writer.Write(NumImages);
                    foreach (TRTexImage8 tex in Images8) { writer.Write(tex.Serialize()); }
                    foreach (TRTexImage16 tex in Images16) { writer.Write(tex.Serialize()); }
                    writer.Write(Unused);
                    writer.Write(NumRooms);
                    foreach (TR2Room room in Rooms) { writer.Write(room.Serialize()); }
                    writer.Write(NumFloorData);
                    foreach (ushort data in FloorData) { writer.Write(data); }
                    writer.Write(NumMeshData);

                    //Because mesh construction still hasnt been resolved, we will
                    //just write the raw mesh data as words for testing
                    foreach (TRMesh mesh in Meshes) { writer.Write(mesh.Serialize()); }
                    //foreach (ushort word in RawMeshData) { writer.Write(word); }

                    writer.Write(NumMeshPointers);
                    foreach (uint ptr in MeshPointers) { writer.Write(ptr); }
                    writer.Write(NumAnimations);
                    foreach (TRAnimation anim in Animations) { writer.Write(anim.Serialize()); }
                    writer.Write(NumStateChanges);
                    foreach (TRStateChange statec in StateChanges) { writer.Write(statec.Serialize()); }
                    writer.Write(NumAnimDispatches);
                    foreach (TRAnimDispatch dispatch in AnimDispatches) { writer.Write(dispatch.Serialize()); }
                    writer.Write(NumAnimCommands);
                    foreach (TRAnimCommand cmd in AnimCommands) { writer.Write(cmd.Serialize()); }
                    writer.Write(NumMeshTrees * 4); //To get the correct number /= 4 is done during read, make sure to reverse it here.
                    foreach (TRMeshTreeNode node in MeshTrees) { writer.Write(node.Serialize()); }
                    writer.Write(NumFrames);
                    foreach (ushort frame in Frames) { writer.Write(frame); }
                    writer.Write(NumModels);
                    foreach (TRModel model in Models) { writer.Write(model.Serialize()); }
                    writer.Write(NumStaticMeshes);
                    foreach (TRStaticMesh mesh in StaticMeshes) { writer.Write(mesh.Serialize()); }
                    writer.Write(NumObjectTextures);
                    foreach (TRObjectTexture tex in ObjectTextures) { writer.Write(tex.Serialize()); }
                    writer.Write(NumSpriteTextures);
                    foreach (TRSpriteTexture tex in SpriteTextures) { writer.Write(tex.Serialize()); }
                    writer.Write(NumSpriteSequences);
                    foreach (TRSpriteSequence sequence in SpriteSequences) { writer.Write(sequence.Serialize()); }
                    writer.Write(NumCameras);
                    foreach (TRCamera cam in Cameras) { writer.Write(cam.Serialize()); }
                    writer.Write(NumSoundSources);
                    foreach (TRSoundSource src in SoundSources) { writer.Write(src.Serialize()); }
                    writer.Write(NumBoxes);
                    foreach (TR2Box box in Boxes) { writer.Write(box.Serialize()); }
                    writer.Write(NumOverlaps);
                    foreach (ushort overlap in Overlaps) { writer.Write(overlap); }
                    //See note in TR2LevelReader re zones
                    foreach (ushort zone in TR2BoxUtilities.FlattenZones(Zones)) { writer.Write(zone); }
                    writer.Write(NumAnimatedTextures);
                    writer.Write((ushort)AnimatedTextures.Length);
                    foreach (TRAnimatedTexture texture in AnimatedTextures) { writer.Write(texture.Serialize()); }
                    writer.Write(NumEntities);
                    foreach (TR2Entity entity in Entities) { writer.Write(entity.Serialize()); }
                    writer.Write(LightMap);
                    writer.Write(NumCinematicFrames);
                    foreach (TRCinematicFrame cineframe in CinematicFrames) { writer.Write(cineframe.Serialize()); }
                    writer.Write(NumDemoData);
                    writer.Write(DemoData);
                    foreach (short sound in SoundMap) { writer.Write(sound); }
                    writer.Write(NumSoundDetails);
                    foreach (TRSoundDetails snddetail in SoundDetails) { writer.Write(snddetail.Serialize()); }
                    writer.Write(NumSampleIndices);
                    foreach (uint index in SampleIndices) { writer.Write(index); }
                }

                return stream.ToArray();
            }
        }
    }
}
