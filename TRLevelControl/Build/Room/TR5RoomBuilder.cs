using System.Diagnostics;
using System.Numerics;
using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TR5RoomBuilder
{
    private static readonly string _xela = "XELA";

    public static List<TR5Room> ReadRooms(TRLevelReader reader)
    {
        uint numRooms = reader.ReadUInt32();
        List<TR5Room> rooms = new();
        for (int i = 0; i < numRooms; i++)
        {
            rooms.Add(ReadRoom(reader));
        }

        return rooms;
    }

    private static TR5Room ReadRoom(TRLevelReader reader)
    {
        string xela = new(reader.ReadChars(4));
        Debug.Assert(xela == _xela);

        uint roomSize = reader.ReadUInt32();
        long roomStart = reader.BaseStream.Position;
        long roomEnd = roomStart + roomSize;

        TR5RoomHeader header = ReadHeader(reader);
        TR5Room room = new()
        {
            AlternateGroup = header.AlternateGroup,
            AlternateRoom = header.AlternateRoom,
            Colour = new(header.Colour),
            Flags = header.Flags,
            Info = header.Info,
            NumXSectors = header.NumXSectors,
            NumZSectors = header.NumZSectors,
            ReverbMode = header.ReverbMode,
            WaterScheme = header.WaterScheme,
            Mesh = new()
            {
                Vertices = new(),
                Rectangles = new(),
                Triangles = new(),
                Sprites = new(),
            }
        };

        long dataStartPos = reader.BaseStream.Position;

        reader.ReadUntil(dataStartPos + header.LightStartOffset);
        room.Lights = ReadLights(reader, header.NumLights);

        reader.ReadUntil(dataStartPos + header.FogBulbStartOffset);
        Queue<TR5FogBulb> fogBulbs = new(ReadFogBulbs(reader, header.NumFogBulbs));

        // Fog bulbs are already earmarked in room.Lights, so we just replace them now with
        // the correct objects.
        for (int i = 0; i < room.Lights.Count && fogBulbs.Count > 0; i++)
        {
            if (room.Lights[i] is TR5FogBulb)
            {
                room.Lights[i] = fogBulbs.Dequeue();
            }
        }

        // TRLEs don't earmark fog bulbs it seems, so we just append any that remain.
        room.Lights.AddRange(fogBulbs);

        reader.ReadUntil(dataStartPos + header.SectorStartOffset);
        room.Sectors = reader.ReadRoomSectors(room.NumXSectors * room.NumZSectors);

        ushort numPortals = reader.ReadUInt16();
        room.Portals = reader.ReadRoomPortals(numPortals);
        reader.ReadUntil(dataStartPos + header.PortalEndOffset);

        room.StaticMeshes = ReadStaticMeshes(reader, header.NumStaticMeshes);

        if (header.NumRoomlets == 0)
        {
            reader.ReadUntil(roomEnd);
            return room;
        }

        reader.ReadUntil(dataStartPos + header.RoomletStartOffset);
        List<TR5Roomlet> roomlets = new();
        for (int i = 0; i < header.NumRoomlets; i++)
        {
            roomlets.Add(new()
            {
                NumVertices = reader.ReadUInt16(),
                NumWaterVertices = reader.ReadUInt16(),
                NumShoreVertices = reader.ReadUInt16(),
                NumRectangles = reader.ReadUInt16(),
                NumTriangles = reader.ReadUInt16(),
                NumWaterRectangles = reader.ReadUInt16(),
                NumWaterTriangles = reader.ReadUInt16(),
            });

            reader.ReadUInt16(); // Filler, always 0
            reader.ReadTR5Vertex(); // Box min
            reader.ReadTR5Vertex(); // Box max
            reader.ReadUInt32s(4); // Filler
        }

        reader.ReadUntil(dataStartPos + header.PolyStartOffset);
        foreach (TR5Roomlet roomlet in roomlets)
        {
            roomlet.Rectangles = reader.ReadMeshFaces(roomlet.NumRectangles, TRFaceType.Rectangle, TRGameVersion.TR5);
            roomlet.Triangles = reader.ReadMeshFaces(roomlet.NumTriangles, TRFaceType.Triangle, TRGameVersion.TR5);
        }

        // Read in the vertices and then squash the roomlets out of existence
        reader.ReadUntil(dataStartPos + header.VerticesStartOffset);
        foreach (TR5Roomlet roomlet in roomlets)
        {
            roomlet.Vertices = ReadVertices(reader, roomlet.NumVertices);

            if (room.Mesh.Vertices.Count > 0)
            {
                // Vertex references will be room relative now, not roomlet
                foreach (TRMeshFace face in roomlet.Rectangles.Concat(roomlet.Triangles))
                {
                    for (int i = 0; i < face.Vertices.Count; i++)
                    {
                        face.Vertices[i] += (ushort)room.Mesh.Vertices.Count;
                    }
                }
            }
            room.Mesh.Vertices.AddRange(roomlet.Vertices);
            room.Mesh.Rectangles.AddRange(ConvertFromMeshFaces(roomlet.Rectangles));
            room.Mesh.Triangles.AddRange(ConvertFromMeshFaces(roomlet.Triangles));
        }

        // Skip to the end and we're done
        reader.ReadUntil(roomEnd);
        return room;
    }

    private static TR5RoomHeader ReadHeader(TRLevelReader reader)
    {
        TR5RoomHeader header = new();

        reader.ReadUInt32(); // Always 0xCDCDCDCD

        header.SectorEndOffset = reader.ReadUInt32();
        header.SectorStartOffset = reader.ReadUInt32();

        reader.ReadUInt32(); // Either 0 or 0xCDCDCDCD

        header.PortalEndOffset = reader.ReadUInt32();
        header.Info = reader.ReadRoomInfo(TRGameVersion.TR5);
        header.NumZSectors = reader.ReadUInt16();
        header.NumXSectors = reader.ReadUInt16();
        header.Colour = reader.ReadUInt32();
        header.NumLights = reader.ReadUInt16();
        header.NumStaticMeshes = reader.ReadUInt16();
        header.ReverbMode = (TRPSXReverbMode)reader.ReadByte();
        header.AlternateGroup = reader.ReadByte();
        header.WaterScheme = reader.ReadUInt16();

        reader.ReadUInt32s(5); // 2 x 0x00007FFF, 2 x 0xCDCDCDCD, 1 x 0xFFFFFFFF

        header.AlternateRoom = reader.ReadInt16();
        header.Flags = (TRRoomFlag)reader.ReadInt16();
        header.NumVertices = reader.ReadUInt32();

        reader.ReadUInt32s(4); // 0, 0, 0xCDCDCDCD, 0
        reader.ReadSingles(3); // Position as floats, not needed
        reader.ReadUInt32s(6); // 4 x 0xCDCDCDCD, then 0 for normal rooms or 0xCDCDCDCD for null rooms, then another 0xCDCDCDCD
        header.NumTriangles = reader.ReadUInt32();
        header.NumRectangles = reader.ReadUInt32();

        header.LightStartOffset = reader.ReadUInt32();
        header.FogBulbStartOffset = reader.ReadUInt32();

        reader.ReadUInt32(); // NumLights again

        header.NumFogBulbs = reader.ReadUInt32();

        reader.ReadSingles(2); // YTop/YBottom floats

        header.NumRoomlets = reader.ReadUInt32();
        header.RoomletStartOffset = reader.ReadUInt32();
        header.VerticesStartOffset = reader.ReadUInt32();
        header.PolyStartOffset = reader.ReadUInt32();

        reader.ReadUInt32s(6); // Poly offset repeated; total vertex size; then 0xCDCDCDCD x 4

        return header;
    }

    private static List<TR5RoomLight> ReadLights(TRLevelReader reader, long numLights)
    {
        List<TR5RoomLight> lights = new();
        for (int i = 0; i < numLights; i++)
        {
            TR5Vertex position = reader.ReadTR5Vertex();
            TR5Colour colour = reader.ReadTR5Colour();

            reader.ReadUInt32(); // 0xCDCDCDCD

            float inner = reader.ReadSingle();
            float outer = reader.ReadSingle();
            float innerAngle = reader.ReadSingle();
            float outerAngle = reader.ReadSingle();
            float range = reader.ReadSingle();

            Vector3 direction = reader.ReadVector3();

            reader.ReadInt32s(6); // Position and direction as int

            TR4RoomLightType type = (TR4RoomLightType)reader.ReadByte();

            reader.ReadBytes(3); // 0xCD

            // Fog bulb IDs will be wrong, but we need to capture them here
            // as they will be later replaced with actual bulb objects.
            TR5RoomLight light;
            if ((int)type == 0xCD)
            {
                light = new TR5FogBulb();
            }
            else
            {
                light = type switch
                {
                    TR4RoomLightType.Sun => new TR5SunLight(),
                    TR4RoomLightType.Point => new TR5PointLight(),
                    TR4RoomLightType.Spot => new TR5SpotLight(),
                    TR4RoomLightType.Shadow => new TR5ShadowLight(),
                    _ => throw new Exception($"Unexpected light type {type}"),
                };
            }

            lights.Add(light);
            light.Position = position;
            light.Colour = colour;
            if (light is TR5GeneralLight generalLight)
            {
                generalLight.Inner = inner;
                generalLight.Outer = outer;
                generalLight.InnerAngle = innerAngle;
                generalLight.OuterAngle = outerAngle;
                generalLight.Range = range;
                generalLight.Direction = direction;
            }
        }

        return lights;
    }

    private static List<TR5FogBulb> ReadFogBulbs(TRLevelReader reader, long numBulbs)
    {
        List<TR5FogBulb> bulbs = new();
        for (int i = 0; i < numBulbs; i++)
        {
            TR5Vertex position = reader.ReadTR5Vertex();
            float radius = reader.ReadSingle();
            reader.ReadSingle(); // Radius^2

            bulbs.Add(new()
            {
                Position = position,
                Radius = radius,
                Density = reader.ReadSingle(),
                Colour = reader.ReadTR5Colour()
            });
        }
        return bulbs;
    }

    private static List<TR5RoomStaticMesh> ReadStaticMeshes(TRLevelReader reader, long numMeshes)
    {
        List<TR5RoomStaticMesh> meshes = new();
        for (int i = 0; i < numMeshes; i++)
        {
            meshes.Add(new()
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Z = reader.ReadInt32(),
                Angle = reader.ReadInt16(),
                Colour = new(reader.ReadUInt16()),
                Unused = reader.ReadUInt16(),
                ID = TR5Type.SceneryBase + reader.ReadUInt16()
            });
        }
        return meshes;
    }

    private static List<TR5RoomVertex> ReadVertices(TRLevelReader reader, long numVertices)
    {
        List<TR5RoomVertex> vertices = new();
        for (int i = 0; i < numVertices; i++)
        {
            // Although stored as floats, all OG values are simply shorts
            TR5Vertex vertex = reader.ReadTR5Vertex();
            vertices.Add(new()
            {
                Vertex = new()
                {
                    X = (short)vertex.X,
                    Y = (short)vertex.Y,
                    Z = (short)vertex.Z,
                },
                Normal = reader.ReadTR5Vertex(),
                Colour = new(reader.ReadUInt32())
            });
        }
        return vertices;
    }

    public static void WriteRooms(TRLevelWriter writer, List<TR5Room> rooms)
    {
        writer.Write((uint)rooms.Count);
        foreach (TR5Room room in rooms)
        {
            WriteRoom(writer, room);
        }
    }

    private static void WriteRoom(TRLevelWriter writer, TR5Room room)
    {
        TR5RoomHeader header = new()
        {
            AlternateGroup = room.AlternateGroup,
            AlternateRoom = room.AlternateRoom,
            Colour = room.Colour.ToARGB(),
            Flags = room.Flags,
            Info = room.Info,
            NumFogBulbs = (uint)room.Lights.Count(l => l is TR5FogBulb),
            NumLights = (ushort)room.Lights.Count,
            NumRectangles = (uint)room.Mesh.Rectangles.Count,
            NumTriangles = (uint)room.Mesh.Triangles.Count,
            NumVertices = (uint)room.Mesh.Vertices.Count,
            NumRoomlets = 1,
            NumStaticMeshes = (ushort)room.StaticMeshes.Count,
            NumXSectors = room.NumXSectors,
            NumZSectors = room.NumZSectors,
            ReverbMode = room.ReverbMode,
            WaterScheme = room.WaterScheme,
        };

        using TRLevelWriter roomWriter = new();
        using TRLevelWriter headerWriter = new();

        WriteRoom(roomWriter, room, header);
        WriteHeader(headerWriter, header);

        byte[] roomData = (roomWriter.BaseStream as MemoryStream).ToArray();
        byte[] headerData = (headerWriter.BaseStream as MemoryStream).ToArray();

        writer.Write(_xela.ToCharArray());
        writer.Write((uint)(roomData.Length + headerData.Length));

        writer.Write(headerData);
        writer.Write(roomData);
    }

    private static void WriteRoom(TRLevelWriter writer, TR5Room room, TR5RoomHeader header)
    {
        header.LightStartOffset = (uint)writer.BaseStream.Position;
        WriteLights(writer, room.Lights);

        header.FogBulbStartOffset = (uint)writer.BaseStream.Position;
        WriteFogBulbs(writer, room.Lights.Where(l => l is TR5FogBulb).Cast<TR5FogBulb>());

        header.SectorStartOffset = (uint)writer.BaseStream.Position;
        writer.Write(room.Sectors);
        header.SectorEndOffset = (uint)writer.BaseStream.Position;

        writer.Write((ushort)room.Portals.Count);
        writer.Write(room.Portals);
        writer.Write((ushort)0xCDCD);
        header.PortalEndOffset = (uint)writer.BaseStream.Position;

        WriteStaticMeshes(writer, room.StaticMeshes);

        header.RoomletStartOffset = (uint)writer.BaseStream.Position;
        writer.Write((ushort)room.Mesh.Vertices.Count);
        writer.Write(Enumerable.Repeat((ushort)0, 2)); // waterVertCount, shoreVertCount
        writer.Write((ushort)room.Mesh.Rectangles.Count);
        writer.Write((ushort)room.Mesh.Triangles.Count);
        writer.Write(Enumerable.Repeat((ushort)0, 3)); // waterRectCount, waterTriCount, filler

        // Min bounds
        writer.Write(new TR5Vertex
        {
            X = TRConsts.Step4,
            Y = room.Info.YTop,
            Z = TRConsts.Step4,
        });
        // Max bounds
        writer.Write(new TR5Vertex
        {
            X = (room.NumXSectors - 1) * TRConsts.Step4 - 1,
            Y = room.Info.YBottom,
            Z = (room.NumZSectors - 1) * TRConsts.Step4 - 1,
        });

        writer.Write(Enumerable.Repeat((uint)0, 4)); // Filler

        header.PolyStartOffset = (uint)writer.BaseStream.Position;
        writer.Write(ConvertToMeshFaces(room.Mesh.Rectangles), TRGameVersion.TR5);
        writer.Write(ConvertToMeshFaces(room.Mesh.Triangles), TRGameVersion.TR5);

        header.VerticesStartOffset = (uint)writer.BaseStream.Position;
        WriteVertices(writer, room.Mesh.Vertices);
    }

    private static void WriteHeader(TRLevelWriter writer, TR5RoomHeader header)
    {
        bool isNullRoom = header.NumVertices == 0;

        writer.Write(0xCDCDCDCD);
        writer.Write(header.SectorEndOffset);
        writer.Write(header.SectorStartOffset);

        writer.Write(0xCDCDCDCD);
        writer.Write(header.PortalEndOffset);

        writer.Write(header.Info, TRGameVersion.TR5);
        writer.Write(header.NumZSectors);
        writer.Write(header.NumXSectors);
        writer.Write(header.Colour);
        writer.Write(header.NumLights);
        writer.Write(header.NumStaticMeshes);
        writer.Write((byte)header.ReverbMode);
        writer.Write(header.AlternateGroup);
        writer.Write(header.WaterScheme);

        writer.Write(new uint[] { 0x00007FFF, 0x00007FFF, 0xCDCDCDCD, 0xCDCDCDCD, 0xFFFFFFFF }); // Filler

        writer.Write(header.AlternateRoom);
        writer.Write((short)header.Flags);

        writer.Write(header.NumVertices);
        writer.Write((uint)0);
        writer.Write((uint)0);
        writer.Write(0xCDCDCDCD);
        writer.Write((uint)0);

        if (isNullRoom)
        {
            writer.Write(Enumerable.Repeat(0xCDCDCDCD, 3));
        }
        else
        {
            writer.Write((float)header.Info.X);
            writer.Write((float)0);
            writer.Write((float)header.Info.Z);
        }

        writer.Write(Enumerable.Repeat(0xCDCDCDCD, 4));
        writer.Write(isNullRoom ? 0xCDCDCDCD : 0);
        writer.Write(0xCDCDCDCD);

        if (isNullRoom)
        {
            writer.Write(Enumerable.Repeat(0xCDCDCDCD, 2));
        }
        else
        {
            writer.Write(header.NumTriangles);
            writer.Write(header.NumRectangles);
        }

        writer.Write(header.LightStartOffset);
        writer.Write(header.FogBulbStartOffset);

        writer.Write((uint)header.NumLights);
        writer.Write(header.NumFogBulbs);

        if (isNullRoom)
        {
            writer.Write(Enumerable.Repeat(0xCDCDCDCD, 2));
        }
        else
        {
            writer.Write((float)header.Info.YTop);
            writer.Write((float)header.Info.YBottom);
        }

        writer.Write(header.NumRoomlets);

        writer.Write(header.RoomletStartOffset);
        writer.Write(header.VerticesStartOffset);
        writer.Write(header.PolyStartOffset);
        writer.Write(header.PolyStartOffset);
        writer.Write(header.NumVertices * 28);

        writer.Write(Enumerable.Repeat(0xCDCDCDCD, 4));
    }

    private static void WriteLights(TRLevelWriter writer, List<TR5RoomLight> lights)
    {
        foreach (TR5RoomLight light in lights)
        {
            if (light is TR5FogBulb)
            {
                // Write a null bulb, the actual objects are handled separately.
                writer.Write(Enumerable.Repeat((float)-431602080, 6)); // Position and colour
                writer.Write(0xCDCDCDCD);                              // Separator
                writer.Write(Enumerable.Repeat((float)-431602080, 8)); // Inner, outer, inRad, outRad, range, direction
                writer.Write(Enumerable.Repeat(-842150451, 6));        // Position and direction as ints
                writer.Write((byte)0xCD);                              // Fake light type
            }
            else
            {
                TR5GeneralLight generalLight = light as TR5GeneralLight;
                writer.Write(generalLight.Position);
                writer.Write(generalLight.Colour.R);
                writer.Write(generalLight.Colour.G);
                writer.Write(generalLight.Colour.B);
                writer.Write(0xCDCDCDCD);
                writer.Write(generalLight.Inner);
                writer.Write(generalLight.Outer);
                writer.Write(generalLight.InnerAngle);
                writer.Write(generalLight.OuterAngle);
                writer.Write(generalLight.Range);
                writer.Write(generalLight.Direction);

                // Position as int
                writer.Write((int)light.Position.X);
                writer.Write((int)light.Position.Y);
                writer.Write((int)light.Position.Z);

                // Direction as int
                writer.Write((int)(generalLight.Direction.X * 16384));
                writer.Write((int)(generalLight.Direction.Y * 16384));
                writer.Write((int)(generalLight.Direction.Z * 16384));

                writer.Write((byte)generalLight.Type);
            }

            writer.Write(Enumerable.Repeat((byte)0xCD, 3)); // Filler
        }
    }

    private static void WriteFogBulbs(TRLevelWriter writer, IEnumerable<TR5FogBulb> fogBulbs)
    {
        foreach (TR5FogBulb bulb in fogBulbs)
        {
            writer.Write(bulb.Position);
            writer.Write(bulb.Radius);
            writer.Write(bulb.Radius * bulb.Radius);
            writer.Write(bulb.Density);
            writer.Write(bulb.Colour.R);
            writer.Write(bulb.Colour.G);
            writer.Write(bulb.Colour.B);
        }
    }

    private static void WriteStaticMeshes(TRLevelWriter writer, List<TR5RoomStaticMesh> meshes)
    {
        foreach (TR5RoomStaticMesh mesh in meshes)
        {
            writer.Write(mesh.X);
            writer.Write((uint)mesh.Y);
            writer.Write(mesh.Z);
            writer.Write(mesh.Angle);
            writer.Write(mesh.Colour.ToRGB555());
            writer.Write(mesh.Unused);
            writer.Write((ushort)(mesh.ID - TR5Type.SceneryBase));
        }
    }

    private static void WriteVertices(TRLevelWriter writer, List<TR5RoomVertex> vertices)
    {
        foreach (TR5RoomVertex vertex in vertices)
        {
            writer.Write((float)vertex.Vertex.X);
            writer.Write((float)vertex.Vertex.Y);
            writer.Write((float)vertex.Vertex.Z);
            writer.Write(vertex.Normal);
            writer.Write(vertex.Colour.ToARGB());
        }
    }

    private static IEnumerable<TRFace> ConvertFromMeshFaces(IEnumerable<TRMeshFace> meshFaces)
    {
        return meshFaces.Select(f => new TRFace
        {
            DoubleSided = f.DoubleSided,
            Texture = f.Texture,
            Type = f.Type,
            UnknownFlag = f.UnknownFlag,
            Vertices = f.Vertices
        });
    }

    private static IEnumerable<TRMeshFace> ConvertToMeshFaces(IEnumerable<TRFace> faces)
    {
        return faces.Select(f => new TRMeshFace
        {
            DoubleSided = f.DoubleSided,
            Texture = f.Texture,
            Type = f.Type,
            UnknownFlag = f.UnknownFlag,
            Vertices = f.Vertices,
        });
    }

    class TR5RoomHeader
    {
        public uint SectorEndOffset { get; set; }
        public uint SectorStartOffset { get; set; }
        public uint PortalEndOffset { get; set; }
        public TRRoomInfo Info { get; set; }
        public ushort NumZSectors { get; set; }
        public ushort NumXSectors { get; set; }
        public uint Colour { get; set; }
        public ushort NumLights { get; set; }
        public ushort NumStaticMeshes { get; set; }
        public TRPSXReverbMode ReverbMode { get; set; }
        public byte AlternateGroup { get; set; }
        public ushort WaterScheme { get; set; }
        public short AlternateRoom { get; set; }
        public TRRoomFlag Flags { get; set; }
        public uint NumVertices { get; set; }
        public uint NumTriangles { get; set; }
        public uint NumRectangles { get; set; }
        public uint LightStartOffset { get; set; }
        public uint FogBulbStartOffset { get; set; }
        public uint NumFogBulbs { get; set; }
        public uint NumRoomlets { get; set; }
        public uint RoomletStartOffset { get; set; }
        public uint VerticesStartOffset { get; set; }
        public uint PolyStartOffset { get; set; }
    }

    class TR5Roomlet
    {
        public ushort NumVertices { get; set; }
        public ushort NumWaterVertices { get; set; }
        public ushort NumShoreVertices { get; set; }
        public ushort NumTriangles { get; set; }
        public ushort NumRectangles { get; set; }
        public ushort NumWaterRectangles { get; set; }
        public ushort NumWaterTriangles { get; set; }
        public List<TRMeshFace> Rectangles { get; set; }
        public List<TRMeshFace> Triangles { get; set; }
        public List<TR5RoomVertex> Vertices { get; set; }
    }
}
