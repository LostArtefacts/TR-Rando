using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelReader.Serialization;

namespace TRLevelReader.Model
{
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

                }

                return stream.ToArray();
            }
        }
    }
}
