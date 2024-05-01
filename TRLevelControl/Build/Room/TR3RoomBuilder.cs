using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TR3RoomBuilder : TRRoomBuilder<TR3Type, TR3Room>
{
    public TR3RoomBuilder()
        : base(TRGameVersion.TR3) { }

    protected override void ReadLights(TR3Room room, TRLevelReader reader)
    {
        room.AmbientIntensity = reader.ReadInt16();
        room.LightMode = reader.ReadInt16();

        ushort numLights = reader.ReadUInt16();
        room.Lights = new();
        for (int j = 0; j < numLights; j++)
        {
            room.Lights.Add(new()
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Z = reader.ReadInt32(),
                Colour = reader.ReadColour(),
                Type = (TR3RoomLightType)reader.ReadByte(),
                LightProperties = reader.ReadInt16s(4)
            });
        }
    }

    protected override void ReadStatics(TR3Room room, TRLevelReader reader)
    {
        ushort numStaticMeshes = reader.ReadUInt16();
        room.StaticMeshes = new();
        for (int j = 0; j < numStaticMeshes; j++)
        {
            room.StaticMeshes.Add(new()
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Z = reader.ReadInt32(),
                Angle = reader.ReadInt16(),
                Colour = reader.ReadUInt16(),
                Unused = reader.ReadUInt16(),
                ID = TR3Type.SceneryBase + reader.ReadUInt16()
            });
        }
    }

    protected override void ReadAdditionalProperties(TR3Room room, TRLevelReader reader)
    {
        room.WaterScheme = reader.ReadByte();
        room.ReverbMode = (TRPSXReverbMode)reader.ReadByte();
        reader.ReadByte();
    }

    protected override void BuildMesh(TR3Room room, TRLevelReader reader, ISpriteProvider<TR3Type> spriteProvider)
    {
        short numVertices = reader.ReadInt16();
        List<TR3RoomVertex> vertices = new();
        for (int i = 0; i < numVertices; i++)
        {
            vertices.Add(new()
            {
                Vertex = reader.ReadVertex(),
                Lighting = reader.ReadInt16(),
                Attributes = reader.ReadUInt16(),
                Colour = reader.ReadUInt16(),
            });
        }

        room.Mesh = new()
        {
            Vertices = vertices,
            Rectangles = ReadFaces(TRFaceType.Rectangle, reader),
            Triangles = ReadFaces(TRFaceType.Triangle, reader),
            Sprites = ReadSprites(reader, spriteProvider),
        };
    }

    protected override void WriteMesh(TR3Room room, TRLevelWriter writer, ISpriteProvider<TR3Type> spriteProvider)
    {
        writer.Write((short)room.Mesh.Vertices.Count);
        foreach (TR3RoomVertex vertex in room.Mesh.Vertices)
        {
            writer.Write(vertex.Vertex);
            writer.Write(vertex.Lighting);
            writer.Write(vertex.Attributes);
            writer.Write(vertex.Colour);
        }

        WriteFaces(room.Mesh.Rectangles, writer);
        WriteFaces(room.Mesh.Triangles, writer);
        WriteSprites(room.Mesh.Sprites, spriteProvider, writer);
    }

    protected override void WriteLights(TR3Room room, TRLevelWriter writer)
    {
        writer.Write(room.AmbientIntensity);
        writer.Write(room.LightMode);

        writer.Write((ushort)room.Lights.Count);
        foreach (TR3RoomLight light in room.Lights)
        {
            writer.Write(light.X);
            writer.Write(light.Y);
            writer.Write(light.Z);
            writer.Write(light.Colour);
            writer.Write((byte)light.Type);
            writer.Write(light.LightProperties);
        }
    }

    protected override void WriteStatics(TR3Room room, TRLevelWriter writer)
    {
        writer.Write((ushort)room.StaticMeshes.Count);
        foreach (TR3RoomStaticMesh mesh in room.StaticMeshes)
        {
            writer.Write(mesh.X);
            writer.Write(mesh.Y);
            writer.Write(mesh.Z);
            writer.Write(mesh.Angle);
            writer.Write(mesh.Colour);
            writer.Write(mesh.Unused);
            writer.Write((ushort)(mesh.ID - TR3Type.SceneryBase));
        }
    }

    protected override void WriteAdditionalProperties(TR3Room room, TRLevelWriter writer)
    {
        writer.Write(room.WaterScheme);
        writer.Write((byte)room.ReverbMode);
        writer.Write((byte)0);
    }
}
