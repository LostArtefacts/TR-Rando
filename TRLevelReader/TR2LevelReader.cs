using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Model;

namespace TRLevelReader
{
    public class TR2LevelReader
    {
        private readonly uint TR2VersionHeader = 0x0000002D;
        private const uint MAX_PALETTE_SIZE = 256;

        private BinaryReader reader;

        public TR2LevelReader()
        {

        }

        public TR2Level ReadLevel(string Filename)
        {
            if (!Filename.ToUpper().Contains("TR2"))
            {
                throw new NotImplementedException("File reader only supports TR2 levels");
            }

            TR2Level level = new TR2Level();
            reader = new BinaryReader(File.Open(Filename, FileMode.Open));
            Log.LogF("File opened");
            Log.LogV("==========Data Log========== " + DateTime.Now);

            //Version
            level.Version = reader.ReadUInt32();
            if (level.Version != TR2VersionHeader)
            {
                throw new NotImplementedException("File reader only suppors TR2 levels");
            }

            Log.LogV("-----Version:");
            Log.LogV(level.Version);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Colour palettes and textures
            level.Palette = PopulateColourPalette(reader.ReadBytes((int)MAX_PALETTE_SIZE * 3));

            level.Palette16 = PopulateColourPalette16(reader.ReadBytes((int)MAX_PALETTE_SIZE * 4));

            level.NumImages = reader.ReadUInt32();

            level.Images8 = new TRTexImage8[level.NumImages];
            level.Images16 = new TRTexImage16[level.NumImages];

            //Initialize the texture arrays
            for (int i = 0; i < level.NumImages; i++)
            {
                level.Images8[i] = new TRTexImage8();
                level.Images16[i] = new TRTexImage16();
            }

            //For each texture8 there are 256 * 256 bytes (65536) we can just do a straight byte read
            for (int i = 0; i < level.NumImages; i++)
            {
                level.Images8[i].Pixels = reader.ReadBytes(256 * 256);
            }

            //For each texture16 there are 256 * 256 * 2 bytes (131072)
            for (int i = 0; i < level.NumImages; i++)
            {
                level.Images16[i].Pixels = new ushort[256 * 256];

                for (int j = 0; j < level.Images16[i].Pixels.Count(); j++)
                {
                    level.Images16[i].Pixels[j] = reader.ReadUInt16();
                }
            }

            //Rooms
            level.Unused = reader.ReadUInt32();
            level.NumRooms = reader.ReadUInt16();
            level.Rooms = new TR2Room[level.NumRooms];

            for (int i = 0; i < level.NumRooms; i++)
            {
                TR2Room room = new TR2Room();

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
                room.SectorList = new TRRoomSector[room.NumXSectors * room.NumZSectors];
                for (int j = 0; j < (room.NumXSectors * room.NumZSectors); j++)
                {
                    room.SectorList[j] = TR2FileReadUtilities.ReadRoomSector(reader);
                }

                //Lighting
                room.AmbientIntensity = reader.ReadInt16();
                room.AmbientIntensity2 = reader.ReadInt16();
                room.LightMode = reader.ReadInt16();
                room.NumLights = reader.ReadUInt16();
                room.Lights = new TR2RoomLight[room.NumLights];
                for (int j = 0; j < room.NumLights; j++)
                {
                    room.Lights[j] = TR2FileReadUtilities.ReadRoomLight(reader);
                }

                //Static meshes
                room.NumStaticMeshes = reader.ReadUInt16();
                room.StaticMeshes = new TR2RoomStaticMesh[room.NumStaticMeshes];
                for (int j = 0; j < room.NumStaticMeshes; j++)
                {
                    room.StaticMeshes[j] = TR2FileReadUtilities.ReadRoomStaticMesh(reader);
                }

                room.AlternateRoom = reader.ReadInt16();
                room.Flags = reader.ReadInt16();

                level.Rooms[i] = room;
            }

            Log.LogV("-----Rooms:");
            Log.LogV(level.Rooms);
            Log.LogV("Position: " + reader.BaseStream.Position);

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
            ushort[] TempMeshData = new ushort[level.NumMeshData];

            for (int i = 0; i < level.NumMeshData; i++)
            {
                TempMeshData[i] = reader.ReadUInt16();
            }

            //Mesh Pointers
            level.NumMeshPointers = reader.ReadUInt32();
            level.MeshPointers = new uint[level.NumMeshPointers];

            for (int i = 0; i < level.NumMeshPointers; i++)
            {
                level.MeshPointers[i] = reader.ReadUInt32();
            }

            //Mesh Construction
            level.Meshes = ConstructMeshData(level.NumMeshData, level.NumMeshPointers, TempMeshData);

            Log.LogV("-----Meshes:");
            Log.LogV(level.Meshes);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Animations
            level.NumAnimations = reader.ReadUInt32();
            level.Animations = new TRAnimation[level.NumAnimations];
            for (int i = 0; i < level.NumAnimations; i++)
            {
                level.Animations[i] = TR2FileReadUtilities.ReadAnimation(reader);
            }

            Log.LogV("-----Animations:");
            Log.LogV(level.Animations);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //State Changes
            level.NumStateChanges = reader.ReadUInt32();
            level.StateChanges = new TRStateChange[level.NumStateChanges];
            for (int i = 0; i < level.NumStateChanges; i++)
            {
                level.StateChanges[i] = TR2FileReadUtilities.ReadStateChange(reader);
            }

            Log.LogV("-----State Changes:");
            Log.LogV(level.StateChanges);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Animation Dispatches
            level.NumAnimDispatches = reader.ReadUInt32();
            level.AnimDispatches = new TRAnimDispatch[level.NumAnimDispatches];
            for (int i = 0; i < level.NumAnimDispatches; i++)
            {
                level.AnimDispatches[i] = TR2FileReadUtilities.ReadAnimDispatch(reader);
            }

            Log.LogV("-----Animation Dispatches:");
            Log.LogV(level.AnimDispatches);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Animation Commands
            level.NumAnimCommands = reader.ReadUInt32();
            level.AnimCommands = new TRAnimCommand[level.NumAnimCommands];
            for (int i = 0; i < level.NumAnimCommands; i++)
            {
                level.AnimCommands[i] = TR2FileReadUtilities.ReadAnimCommand(reader);
            }

            Log.LogV("-----Animation Commands:");
            Log.LogV(level.AnimCommands);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Mesh Trees
            level.NumMeshTrees = reader.ReadUInt32();
            level.MeshTrees = new TRMeshTreeNode[level.NumMeshTrees];
            for (int i = 0; i < level.NumMeshTrees; i++)
            {
                level.MeshTrees[i] = TR2FileReadUtilities.ReadMeshTreeNode(reader);
            }

            Log.LogV("-----Mesh Trees:");
            Log.LogV(level.MeshTrees);
            Log.LogV("Position: " + reader.BaseStream.Position);

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

            Log.LogV("-----Models:");
            Log.LogV(level.Models);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Static Meshes
            level.NumStaticMeshes = reader.ReadUInt32();
            level.StaticMeshes = new TRStaticMesh[level.NumStaticMeshes];

            for (int i = 0; i < level.NumStaticMeshes; i++)
            {
                level.StaticMeshes[i] = TR2FileReadUtilities.ReadStaticMesh(reader);
            }

            Log.LogV("-----Static Meshes:");
            Log.LogV(level.StaticMeshes);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Object Textures
            level.NumObjectTextures = reader.ReadUInt32();
            level.ObjectTextures = new TRObjectTexture[level.NumObjectTextures];

            for (int i = 0; i < level.NumObjectTextures; i++)
            {
                level.ObjectTextures[i] = TR2FileReadUtilities.ReadObjectTexture(reader);
            }

            Log.LogV("-----Object Textures:");
            Log.LogV(level.ObjectTextures);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Sprite Textures
            level.NumSpriteTextures = reader.ReadUInt32();
            level.SpriteTextures = new TRSpriteTexture[level.NumSpriteTextures];

            for (int i = 0; i < level.NumSpriteTextures; i++)
            {
                level.SpriteTextures[i] = TR2FileReadUtilities.ReadSpriteTexture(reader);
            }

            Log.LogV("-----Sprite Textures:");
            Log.LogV(level.SpriteTextures);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Sprite Sequences
            level.NumSpriteSequences = reader.ReadUInt32();
            level.SpriteSequences = new TRSpriteSequence[level.NumSpriteSequences];

            for (int i = 0; i < level.NumSpriteSequences; i++)
            {
                level.SpriteSequences[i] = TR2FileReadUtilities.ReadSpriteSequence(reader);
            }

            Log.LogV("-----Sprite Sequences:");
            Log.LogV(level.SpriteSequences);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Cameras
            level.NumCameras = reader.ReadUInt32();
            level.Cameras = new TRCamera[level.NumCameras];

            for (int i = 0; i < level.NumCameras; i++)
            {
                level.Cameras[i] = TR2FileReadUtilities.ReadCamera(reader);
            }

            Log.LogV("-----Cameras:");
            Log.LogV(level.Cameras);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Sound Sources
            level.NumSoundSources = reader.ReadUInt32();
            level.SoundSources = new TRSoundSource[level.NumSoundSources];

            for (int i = 0; i < level.NumSoundSources; i++)
            {
                level.SoundSources[i] = TR2FileReadUtilities.ReadSoundSource(reader);
            }

            Log.LogV("-----Sound Sources:");
            Log.LogV(level.SoundSources);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Boxes
            level.NumBoxes = reader.ReadUInt32();
            level.Boxes = new TR2Box[level.NumBoxes];

            for (int i = 0; i < level.NumBoxes; i++)
            {
                level.Boxes[i] = TR2FileReadUtilities.ReadBox(reader);
            }

            Log.LogV("-----Boxes:");
            Log.LogV(level.Boxes);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Overlaps & Zones
            level.NumOverlaps = reader.ReadUInt32();
            level.Overlaps = new ushort[level.NumOverlaps];
            level.Zones = new short[10 * level.NumBoxes];

            for (int i = 0; i < level.NumOverlaps; i++)
            {
                level.Overlaps[i] = reader.ReadUInt16();
            }

            for (int i = 0; i < level.Zones.Count(); i++)
            {
                level.Zones[i] = reader.ReadInt16();
            }

            //Animated Textures
            level.NumAnimatedTextures = reader.ReadUInt32();
            level.AnimatedTextures = new ushort[level.NumAnimatedTextures];

            for (int i = 0; i < level.NumAnimatedTextures; i++)
            {
                level.AnimatedTextures[i] = reader.ReadUInt16();
            }

            //Entities
            level.NumEntities = reader.ReadUInt32();
            level.Entities = new TR2Entity[level.NumEntities];

            for (int i = 0; i < level.NumEntities; i++)
            {
                level.Entities[i] = TR2FileReadUtilities.ReadEntity(reader);
            }

            Log.LogV("-----Entities:");
            Log.LogV(level.Entities);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Light Map - 32 * 256 = 8192 bytes
            level.LightMap = new byte[32 * 256];

            for (int i = 0; i < level.LightMap.Count(); i++)
            {
                level.LightMap[i] = reader.ReadByte();
            }

            //Cinematic Frames
            level.NumCinematicFrames = reader.ReadUInt16();
            level.CinematicFrames = new TRCinematicFrame[level.NumCinematicFrames];

            for (int i = 0; i < level.NumCinematicFrames; i++)
            {
                level.CinematicFrames[i] = TR2FileReadUtilities.ReadCinematicFrame(reader);
            }

            Log.LogV("-----Cinematic Frames:");
            Log.LogV(level.CinematicFrames);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Demo Data
            level.NumDemoData = reader.ReadUInt16();
            level.DemoData = new byte[level.NumDemoData];

            for (int i = 0; i < level.NumDemoData; i++)
            {
                level.DemoData[i] = reader.ReadByte();
            }

            //Sound Map (370 shorts = 740 bytes) & Sound Details
            level.SoundMap = new short[370];
            level.NumSoundDetails = reader.ReadUInt32();
            level.SoundDetails = new TRSoundDetails[level.NumSoundDetails];

            for (int i = 0; i < level.SoundMap.Count(); i++)
            {
                level.SoundMap[i] = reader.ReadInt16();
            }

            for (int i = 0; i < level.NumSoundDetails; i++)
            {
                level.SoundDetails[i] = TR2FileReadUtilities.ReadSoundDetails(reader);
            }

            Log.LogV("-----Sound Details:");
            Log.LogV(level.SoundDetails);
            Log.LogV("Position: " + reader.BaseStream.Position);

            //Samples
            level.NumSampleIndices = reader.ReadUInt32();
            level.SampleIndices = new uint[level.NumSampleIndices];

            for (int i = 0; i < level.NumSampleIndices; i++)
            {
                level.SampleIndices[i] = reader.ReadUInt32();
            }

            Log.LogV("Bytes Read: " + reader.BaseStream.Position.ToString() + "/" + reader.BaseStream.Length.ToString());

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

        private TRRoomData ConvertToRoomData(TR2Room room)
        {
            int RoomDataOffset = 0;

            //Grab detailed room data
            TRRoomData RoomData = new TRRoomData();

            //Room vertices
            RoomData.NumVertices = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
            RoomData.Vertices = new TR2RoomVertex[RoomData.NumVertices];

            RoomDataOffset++;

            for (int j = 0; j < RoomData.NumVertices; j++)
            {
                TR2RoomVertex vertex = new TR2RoomVertex()
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
                vertex.Attributes = room.Data[RoomDataOffset];
                RoomDataOffset++;
                vertex.Lighting2 = UnsafeConversions.UShortToShort(room.Data[RoomDataOffset]);
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

        private TRMesh[] ConstructMeshData(uint DataCount, uint NumPointers, ushort[] MeshData)
        {
            //Track where we are in mesh data
            int MeshDataOffset = 0;

            //Temp storage for forming an int from two ushorts 
            ushort LowBytes;
            ushort HighBytes;

            //We know the amount of meshes via NumPointers and we know amount of mesh data via DataCount
            //The mesh data as uint16s/words are stored in MeshData

            //We need to pack that data into the objects
            TRMesh[] meshes = new TRMesh[NumPointers];

            for (int i = 0; i < NumPointers; i++)
            {
                TRMesh mesh = new TRMesh();

                //Centre
                TRVertex centre = new TRVertex();

                centre.X = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                centre.Y = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                centre.Z = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;

                mesh.Centre = centre;

                //Coll Radius
                LowBytes = MeshData[MeshDataOffset];
                MeshDataOffset++;
                HighBytes = MeshData[MeshDataOffset];
                MeshDataOffset++;

                mesh.CollRadius = LowBytes | HighBytes;

                //Vertices
                mesh.NumVetices = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.Vertices = new TRVertex[mesh.NumVetices];

                for (int j = 0; j < mesh.NumVetices; j++)
                {
                    TRVertex v = new TRVertex();

                    v.X = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                    MeshDataOffset++;
                    v.Y = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                    MeshDataOffset++;
                    v.Z = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                    MeshDataOffset++;

                    mesh.Vertices[j] = v;
                }

                //Lights or Normals
                mesh.NumNormals = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;

                if (mesh.NumNormals > 0)
                {
                    mesh.Normals = new TRVertex[mesh.NumNormals];

                    for (int j = 0; j < mesh.NumNormals; j++)
                    {
                        TRVertex v = new TRVertex();

                        v.X = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                        MeshDataOffset++;
                        v.Y = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                        MeshDataOffset++;
                        v.Z = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                        MeshDataOffset++;

                        mesh.Normals[j] = v;
                    }
                }
                else
                {
                    mesh.Lights = new short[Math.Abs(mesh.NumNormals)];

                    for (int j = 0; j < mesh.Lights.Count(); j++)
                    {
                        mesh.Lights[j] = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                        MeshDataOffset++;
                    }
                }

                //Textured Rectangles
                mesh.NumTexturedRectangles = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.TexturedRectangles = new TRFace4[mesh.NumTexturedRectangles];

                for (int j = 0; j < mesh.NumTexturedRectangles; j++)
                {
                    TRFace4 face = new TRFace4();

                    face.Vertices = new ushort[4];

                    face.Vertices[0] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[1] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[2] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[3] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Texture = MeshData[MeshDataOffset];
                    MeshDataOffset++;

                    mesh.TexturedRectangles[j] = face;
                }

                //Textured Triangles
                mesh.NumTexturedTriangles = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.TexturedTriangles = new TRFace3[mesh.NumTexturedTriangles];

                for (int j = 0; j < mesh.NumTexturedTriangles; j++)
                {
                    TRFace3 face = new TRFace3();

                    face.Vertices = new ushort[3];

                    face.Vertices[0] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[1] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[2] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Texture = MeshData[MeshDataOffset];
                    MeshDataOffset++;

                    mesh.TexturedTriangles[j] = face;
                }

                //Coloured Rectangles
                mesh.NumColouredRectangles = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.ColouredRectangles = new TRFace4[mesh.NumColouredRectangles];

                for (int j = 0; j < mesh.NumColouredRectangles; j++)
                {
                    TRFace4 face = new TRFace4();

                    face.Vertices = new ushort[4];

                    face.Vertices[0] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[1] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[2] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[3] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Texture = MeshData[MeshDataOffset];
                    MeshDataOffset++;

                    mesh.ColouredRectangles[j] = face;
                }

                //Coloured Triangles
                mesh.NumColouredTriangles = UnsafeConversions.UShortToShort(MeshData[MeshDataOffset]);
                MeshDataOffset++;
                mesh.ColouredTriangles = new TRFace3[mesh.NumColouredTriangles];

                for (int j = 0; j < mesh.NumColouredTriangles; j++)
                {
                    TRFace3 face = new TRFace3();

                    face.Vertices = new ushort[3];

                    face.Vertices[0] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[1] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Vertices[2] = MeshData[MeshDataOffset];
                    MeshDataOffset++;
                    face.Texture = MeshData[MeshDataOffset];
                    MeshDataOffset++;

                    mesh.ColouredTriangles[j] = face;
                }

                meshes[i] = mesh;
            }

            //The offset should match the total amount of data.
            Debug.Assert(MeshDataOffset == DataCount);

            return meshes;
        }
    }
}
