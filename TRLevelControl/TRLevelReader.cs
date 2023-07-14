using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Helpers;
using TRLevelReader.Model;

namespace TRLevelReader
{
    public class TR1LevelReader
    {
        private const uint MAX_PALETTE_SIZE = 256;

        private BinaryReader reader;

        public TR1LevelReader()
        {

        }

        public TRLevel ReadLevel(string Filename)
        {
            if (!Filename.ToUpper().Contains("PHD"))
            {
                throw new NotImplementedException("File reader only supports TR1 levels");
            }

            TRLevel level = new TRLevel();
            reader = new BinaryReader(File.Open(Filename, FileMode.Open));

            //Version
            level.Version = reader.ReadUInt32();
            if (level.Version != Versions.TR1)
            {
                throw new NotImplementedException("File reader only suppors TR1 levels");
            }

            level.NumImages = reader.ReadUInt32();
            level.Images8 = new TRTexImage8[level.NumImages];

            //Initialize the texture arrays
            for (int i = 0; i < level.NumImages; i++)
            {
                level.Images8[i] = new TRTexImage8();
            }

            //For each texture8 there are 256 * 256 bytes (65536) we can just do a straight byte read
            for (int i = 0; i < level.NumImages; i++)
            {
                level.Images8[i].Pixels = reader.ReadBytes(256 * 256);
            }

            //Rooms
            level.Unused = reader.ReadUInt32();
            level.NumRooms = reader.ReadUInt16();
            level.Rooms = new TRRoom[level.NumRooms];

            for (int i = 0; i < level.NumRooms; i++)
            {
                TRRoom room = new TRRoom();

                //Grab info
                room.Info = new TRRoomInfo
                {
                    X = reader.ReadInt32(),
                    Z = reader.ReadInt32(),
                    YBottom = reader.ReadInt32(),
                    YTop = reader.ReadInt32()
                };

                //Grab data
                room.NumDataWords = reader.ReadUInt32();
                room.Data = new ushort[room.NumDataWords];
                for (int j = 0; j < room.NumDataWords; j++)
                {
                    room.Data[j] = reader.ReadUInt16();
                }

                //Store what we just read
                room.RoomData = ConvertToRoomData(room);

                //Portals
                room.NumPortals = reader.ReadUInt16();
                room.Portals = new TRRoomPortal[room.NumPortals];
                for (int j = 0; j < room.NumPortals; j++)
                {
                    room.Portals[j] = TR2FileReadUtilities.ReadRoomPortal(reader);
                }

                //Sectors
                room.NumZSectors = reader.ReadUInt16();
                room.NumXSectors = reader.ReadUInt16();
                room.Sectors = new TRRoomSector[room.NumXSectors * room.NumZSectors];
                for (int j = 0; j < (room.NumXSectors * room.NumZSectors); j++)
                {
                    room.Sectors[j] = TR2FileReadUtilities.ReadRoomSector(reader);
                }

                //Lighting
                room.AmbientIntensity = reader.ReadInt16();
                room.NumLights = reader.ReadUInt16();
                room.Lights = new TRRoomLight[room.NumLights];
                for (int j = 0; j < room.NumLights; j++)
                {
                    room.Lights[j] = TRFileReadUtilities.ReadRoomLight(reader);
                }

                //Static meshes
                room.NumStaticMeshes = reader.ReadUInt16();
                room.StaticMeshes = new TRRoomStaticMesh[room.NumStaticMeshes];
                for (int j = 0; j < room.NumStaticMeshes; j++)
                {
                    room.StaticMeshes[j] = TRFileReadUtilities.ReadRoomStaticMesh(reader);
                }

                room.AlternateRoom = reader.ReadInt16();
                room.Flags = reader.ReadInt16();

                level.Rooms[i] = room;
            }

            //Floordata
            level.NumFloorData = reader.ReadUInt32();
            level.FloorData = new ushort[level.NumFloorData];

            for (int i = 0; i < level.NumFloorData; i++)
            {
                level.FloorData[i] = reader.ReadUInt16();
            }

            //Mesh Data
            //This tells us how much mesh data (# of words/uint16s) coming up
            //just like the rooms previously.
            level.NumMeshData = reader.ReadUInt32();
            level.RawMeshData = new ushort[level.NumMeshData];

            for (int i = 0; i < level.NumMeshData; i++)
            {
                level.RawMeshData[i] = reader.ReadUInt16();
            }

            //Mesh Pointers
            level.NumMeshPointers = reader.ReadUInt32();
            level.MeshPointers = new uint[level.NumMeshPointers];

            for (int i = 0; i < level.NumMeshPointers; i++)
            {
                level.MeshPointers[i] = reader.ReadUInt32();
            }

            //Mesh Construction
            //level.Meshes = ConstructMeshData(level.NumMeshData, level.NumMeshPointers, level.RawMeshData);
            level.Meshes = ConstructMeshData(level.MeshPointers, level.RawMeshData);

            //Animations
            level.NumAnimations = reader.ReadUInt32();
            level.Animations = new TRAnimation[level.NumAnimations];
            for (int i = 0; i < level.NumAnimations; i++)
            {
                level.Animations[i] = TR2FileReadUtilities.ReadAnimation(reader);
            }

            //State Changes
            level.NumStateChanges = reader.ReadUInt32();
            level.StateChanges = new TRStateChange[level.NumStateChanges];
            for (int i = 0; i < level.NumStateChanges; i++)
            {
                level.StateChanges[i] = TR2FileReadUtilities.ReadStateChange(reader);
            }

            //Animation Dispatches
            level.NumAnimDispatches = reader.ReadUInt32();
            level.AnimDispatches = new TRAnimDispatch[level.NumAnimDispatches];
            for (int i = 0; i < level.NumAnimDispatches; i++)
            {
                level.AnimDispatches[i] = TR2FileReadUtilities.ReadAnimDispatch(reader);
            }

            //Animation Commands
            level.NumAnimCommands = reader.ReadUInt32();
            level.AnimCommands = new TRAnimCommand[level.NumAnimCommands];
            for (int i = 0; i < level.NumAnimCommands; i++)
            {
                level.AnimCommands[i] = TR2FileReadUtilities.ReadAnimCommand(reader);
            }

            //Mesh Trees
            level.NumMeshTrees = reader.ReadUInt32();
            level.NumMeshTrees /= 4;
            level.MeshTrees = new TRMeshTreeNode[level.NumMeshTrees];
            for (int i = 0; i < level.NumMeshTrees; i++)
            {
                level.MeshTrees[i] = TR2FileReadUtilities.ReadMeshTreeNode(reader);
            }

            //Frames
            level.NumFrames = reader.ReadUInt32();
            level.Frames = new ushort[level.NumFrames];
            for (int i = 0; i < level.NumFrames; i++)
            {
                level.Frames[i] = reader.ReadUInt16();
            }

            //Models
            level.NumModels = reader.ReadUInt32();
            level.Models = new TRModel[level.NumModels];

            for (int i = 0; i < level.NumModels; i++)
            {
                level.Models[i] = TR2FileReadUtilities.ReadModel(reader);
            }

            //Static Meshes
            level.NumStaticMeshes = reader.ReadUInt32();
            level.StaticMeshes = new TRStaticMesh[level.NumStaticMeshes];

            for (int i = 0; i < level.NumStaticMeshes; i++)
            {
                level.StaticMeshes[i] = TR2FileReadUtilities.ReadStaticMesh(reader);
            }

            //Object Textures
            level.NumObjectTextures = reader.ReadUInt32();
            level.ObjectTextures = new TRObjectTexture[level.NumObjectTextures];

            for (int i = 0; i < level.NumObjectTextures; i++)
            {
                level.ObjectTextures[i] = TR2FileReadUtilities.ReadObjectTexture(reader);
            }

            //Sprite Textures
            level.NumSpriteTextures = reader.ReadUInt32();
            level.SpriteTextures = new TRSpriteTexture[level.NumSpriteTextures];

            for (int i = 0; i < level.NumSpriteTextures; i++)
            {
                level.SpriteTextures[i] = TR2FileReadUtilities.ReadSpriteTexture(reader);
            }

            //Sprite Sequences
            level.NumSpriteSequences = reader.ReadUInt32();
            level.SpriteSequences = new TRSpriteSequence[level.NumSpriteSequences];

            for (int i = 0; i < level.NumSpriteSequences; i++)
            {
                level.SpriteSequences[i] = TR2FileReadUtilities.ReadSpriteSequence(reader);
            }

            //Cameras
            level.NumCameras = reader.ReadUInt32();
            level.Cameras = new TRCamera[level.NumCameras];

            for (int i = 0; i < level.NumCameras; i++)
            {
                level.Cameras[i] = TR2FileReadUtilities.ReadCamera(reader);
            }

            //Sound Sources
            level.NumSoundSources = reader.ReadUInt32();
            level.SoundSources = new TRSoundSource[level.NumSoundSources];

            for (int i = 0; i < level.NumSoundSources; i++)
            {
                level.SoundSources[i] = TR2FileReadUtilities.ReadSoundSource(reader);
            }

            //Boxes
            level.NumBoxes = reader.ReadUInt32();
            level.Boxes = new TRBox[level.NumBoxes];

            for (int i = 0; i < level.NumBoxes; i++)
            {
                level.Boxes[i] = TRFileReadUtilities.ReadBox(reader);
            }

            //Overlaps & Zones
            level.NumOverlaps = reader.ReadUInt32();
            level.Overlaps = new ushort[level.NumOverlaps];

            for (int i = 0; i < level.NumOverlaps; i++)
            {
                level.Overlaps[i] = reader.ReadUInt16();
            }

            ushort[] zoneData = new ushort[level.NumBoxes * 6];
            for (int i = 0; i < zoneData.Length; i++)
            {
                zoneData[i] = reader.ReadUInt16();
            }
            level.Zones = TR1BoxUtilities.ReadZones(level.NumBoxes, zoneData);

            //Animated Textures - the data stores the total number of ushorts to read (NumAnimatedTextures)
            //followed by a ushort to describe the number of actual texture group objects.
            level.NumAnimatedTextures = reader.ReadUInt32();
            level.AnimatedTextures = new TRAnimatedTexture[reader.ReadUInt16()];
            for (int i = 0; i < level.AnimatedTextures.Length; i++)
            {
                level.AnimatedTextures[i] = TR2FileReadUtilities.ReadAnimatedTexture(reader);
            }

            //Entities
            level.NumEntities = reader.ReadUInt32();
            level.Entities = new TREntity[level.NumEntities];

            for (int i = 0; i < level.NumEntities; i++)
            {
                level.Entities[i] = TRFileReadUtilities.ReadEntity(reader);
            }

            //Light Map - 32 * 256 = 8192 bytes
            level.LightMap = new byte[32 * 256];

            for (int i = 0; i < level.LightMap.Count(); i++)
            {
                level.LightMap[i] = reader.ReadByte();
            }

            level.Palette = PopulateColourPalette(reader.ReadBytes((int)MAX_PALETTE_SIZE * 3));

            //Cinematic Frames
            level.NumCinematicFrames = reader.ReadUInt16();
            level.CinematicFrames = new TRCinematicFrame[level.NumCinematicFrames];

            for (int i = 0; i < level.NumCinematicFrames; i++)
            {
                level.CinematicFrames[i] = TR2FileReadUtilities.ReadCinematicFrame(reader);
            }

            //Demo Data
            level.NumDemoData = reader.ReadUInt16();
            level.DemoData = new byte[level.NumDemoData];

            for (int i = 0; i < level.NumDemoData; i++)
            {
                level.DemoData[i] = reader.ReadByte();
            }

            //Sound Map & Sound Details
            level.SoundMap = new short[256];

            for (int i = 0; i < level.SoundMap.Count(); i++)
            {
                level.SoundMap[i] = reader.ReadInt16();
            }

            level.NumSoundDetails = reader.ReadUInt32();
            level.SoundDetails = new TRSoundDetails[level.NumSoundDetails];

            for (int i = 0; i < level.NumSoundDetails; i++)
            {
                level.SoundDetails[i] = TR2FileReadUtilities.ReadSoundDetails(reader);
            }

            //Samples
            level.NumSamples = reader.ReadUInt32();
            level.Samples = new byte[level.NumSamples];

            for (int i = 0; i < level.NumSamples; i++)
            {
                level.Samples[i] = reader.ReadByte();
            }

            level.NumSampleIndices = reader.ReadUInt32();
            level.SampleIndices = new uint[level.NumSampleIndices];

            for (int i = 0; i < level.NumSampleIndices; i++)
            {
                level.SampleIndices[i] = reader.ReadUInt32();
            }

            Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);

            reader.Close();

            return level;
        }

        private static TRColour[] PopulateColourPalette(byte[] palette)
        {
            TRColour[] colourPalette = new TRColour[MAX_PALETTE_SIZE];

            int ci = 0;

            for (int i = 0; i < MAX_PALETTE_SIZE; i++)
            {
                TRColour col = new TRColour();

                col.Red = palette[ci];
                ci++;

                col.Green = palette[ci];
                ci++;

                col.Blue = palette[ci];
                ci++;

                colourPalette[i] = col;
            }

            return colourPalette;
        }

        private TRColour4[] PopulateColourPalette16(byte[] palette)
        {
            TRColour4[] colourPalette = new TRColour4[MAX_PALETTE_SIZE];

            int ci = 0;

            for (int i = 0; i < MAX_PALETTE_SIZE; i++)
            {
                TRColour4 col = new TRColour4();

                col.Red = palette[ci];
                ci++;

                col.Green = palette[ci];
                ci++;

                col.Blue = palette[ci];
                ci++;

                col.Unused = palette[ci];
                ci++;

                colourPalette[i] = col;
            }

            return colourPalette;
        }

        private TRRoomData ConvertToRoomData(TRRoom room)
        {
            int RoomDataOffset = 0;

            //Grab detailed room data
            TRRoomData RoomData = new TRRoomData();

            //Room vertices
            RoomData.NumVertices = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomData.Vertices = new TRRoomVertex[RoomData.NumVertices];

            RoomDataOffset++;

            for (int j = 0; j < RoomData.NumVertices; j++)
            {
                TRRoomVertex vertex = new TRRoomVertex()
                {
                    Vertex = new TRVertex()
                };

                vertex.Vertex.X = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;
                vertex.Vertex.Y = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;
                vertex.Vertex.Z = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;
                vertex.Lighting = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;

                RoomData.Vertices[j] = vertex;
            }

            //Room rectangles
            RoomData.NumRectangles = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomData.Rectangles = new TRFace4[RoomData.NumRectangles];

            RoomDataOffset++;

            for (int j = 0; j < RoomData.NumRectangles; j++)
            {
                TRFace4 face = new TRFace4();

                face.Vertices = new ushort[4];
                face.Vertices[0] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Vertices[1] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Vertices[2] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Vertices[3] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Texture = room.Data[RoomDataOffset];
                RoomDataOffset++;

                RoomData.Rectangles[j] = face;
            }

            //Room triangles
            RoomData.NumTriangles = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomData.Triangles = new TRFace3[RoomData.NumTriangles];

            RoomDataOffset++;

            for (int j = 0; j < RoomData.NumTriangles; j++)
            {
                TRFace3 face = new TRFace3();

                face.Vertices = new ushort[3];
                face.Vertices[0] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Vertices[1] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Vertices[2] = room.Data[RoomDataOffset];
                RoomDataOffset++;
                face.Texture = room.Data[RoomDataOffset];
                RoomDataOffset++;

                RoomData.Triangles[j] = face;
            }

            //Room sprites
            RoomData.NumSprites = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomData.Sprites = new TRRoomSprite[RoomData.NumSprites];

            RoomDataOffset++;

            for (int j = 0; j < RoomData.NumSprites; j++)
            {
                TRRoomSprite face = new TRRoomSprite();

                face.Vertex = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;
                face.Texture = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
                RoomDataOffset++;

                RoomData.Sprites[j] = face;
            }

            Debug.Assert(RoomDataOffset == room.NumDataWords);

            return RoomData;
        }

        private TRMesh[] ConstructMeshData(uint[] meshPointers, ushort[] rawMeshData)
        {
            byte[] target = new byte[rawMeshData.Length * 2];
            Buffer.BlockCopy(rawMeshData, 0, target, 0, target.Length);

            // The mesh pointer list can contain duplicates so we must make
            // sure to iterate over distinct values only
            meshPointers = meshPointers.Distinct().ToArray();

            List<TRMesh> meshes = new List<TRMesh>();

            using (MemoryStream ms = new MemoryStream(target))
            using (BinaryReader br = new BinaryReader(ms))
            {
                for (int i = 0; i < meshPointers.Length; i++)
                {
                    TRMesh mesh = new TRMesh();
                    meshes.Add(mesh);

                    uint meshPointer = meshPointers[i];
                    br.BaseStream.Position = meshPointer;

                    //Pointer
                    mesh.Pointer = meshPointer;

                    //Centre
                    mesh.Centre = TR2FileReadUtilities.ReadVertex(br);

                    //CollRadius
                    mesh.CollRadius = br.ReadInt32();

                    //Vertices
                    mesh.NumVertices = br.ReadInt16();
                    mesh.Vertices = new TRVertex[mesh.NumVertices];
                    for (int j = 0; j < mesh.NumVertices; j++)
                    {
                        mesh.Vertices[j] = TR2FileReadUtilities.ReadVertex(br);
                    }

                    //Lights or Normals
                    mesh.NumNormals = br.ReadInt16();
                    if (mesh.NumNormals > 0)
                    {
                        mesh.Normals = new TRVertex[mesh.NumNormals];
                        for (int j = 0; j < mesh.NumNormals; j++)
                        {
                            mesh.Normals[j] = TR2FileReadUtilities.ReadVertex(br);
                        }
                    }
                    else
                    {
                        mesh.Lights = new short[Math.Abs(mesh.NumNormals)];
                        for (int j = 0; j < mesh.Lights.Length; j++)
                        {
                            mesh.Lights[j] = br.ReadInt16();
                        }
                    }

                    //Textured Rectangles
                    mesh.NumTexturedRectangles = br.ReadInt16();
                    mesh.TexturedRectangles = new TRFace4[mesh.NumTexturedRectangles];
                    for (int j = 0; j < mesh.NumTexturedRectangles; j++)
                    {
                        mesh.TexturedRectangles[j] = TR2FileReadUtilities.ReadTRFace4(br);
                    }

                    //Textured Triangles
                    mesh.NumTexturedTriangles = br.ReadInt16();
                    mesh.TexturedTriangles = new TRFace3[mesh.NumTexturedTriangles];
                    for (int j = 0; j < mesh.NumTexturedTriangles; j++)
                    {
                        mesh.TexturedTriangles[j] = TR2FileReadUtilities.ReadTRFace3(br);
                    }

                    //Coloured Rectangles
                    mesh.NumColouredRectangles = br.ReadInt16();
                    mesh.ColouredRectangles = new TRFace4[mesh.NumColouredRectangles];
                    for (int j = 0; j < mesh.NumColouredRectangles; j++)
                    {
                        mesh.ColouredRectangles[j] = TR2FileReadUtilities.ReadTRFace4(br);
                    }

                    //Coloured Triangles
                    mesh.NumColouredTriangles = br.ReadInt16();
                    mesh.ColouredTriangles = new TRFace3[mesh.NumColouredTriangles];
                    for (int j = 0; j < mesh.NumColouredTriangles; j++)
                    {
                        mesh.ColouredTriangles[j] = TR2FileReadUtilities.ReadTRFace3(br);
                    }

                    // There may be alignment padding at the end of the mesh, but rather than
                    // storing it, when the mesh is serialized the alignment should be considered.
                    // It seems to be 4-byte alignment for mesh data. The basestream position is
                    // moved to the next pointer in the next iteration, so we don't need to process
                    // the additional data here.
                    // See https://www.tombraiderforums.com/archive/index.php/t-215247.html
                }
            }

            return meshes.ToArray();
        }
    }
}
