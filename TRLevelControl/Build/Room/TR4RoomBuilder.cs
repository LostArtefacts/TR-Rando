using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TR4RoomBuilder : TRRoomBuilder<TR4Type, TR4Room>
{
    public TR4RoomBuilder()
        : base(TRGameVersion.TR4) { }

    protected override void ReadLights(TR4Room room, TRLevelReader reader)
    {
        room.Colour = new(reader.ReadUInt32());

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
                Type = (TR4RoomLightType)reader.ReadByte(),
                Intensity = reader.ReadUInt16(),
                Inner = reader.ReadSingle(),
                Outer = reader.ReadSingle(),
                Length = reader.ReadSingle(),
                CutOff = reader.ReadSingle(),
                Direction = reader.ReadVector3()
            });
        }
    }

    protected override void ReadStatics(TR4Room room, TRLevelReader reader)
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
                ID = TR4Type.SceneryBase + reader.ReadUInt16()
            });
        }
    }

    protected override void ReadAdditionalProperties(TR4Room room, TRLevelReader reader)
    {
        room.WaterScheme = reader.ReadByte();
        room.ReverbMode = (TRPSXReverbMode)reader.ReadByte();
        room.AlternateGroup = reader.ReadByte();
    }

    protected override void BuildMesh(TR4Room room, TRLevelReader reader, ISpriteProvider<TR4Type> spriteProvider)
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

    protected override void WriteMesh(TR4Room room, TRLevelWriter writer, ISpriteProvider<TR4Type> spriteProvider)
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

    protected override void WriteLights(TR4Room room, TRLevelWriter writer)
    {
        writer.Write(room.Colour.ToARGB());

        writer.Write((ushort)room.Lights.Count);
        foreach (TR4RoomLight light in room.Lights)
        {
            writer.Write(light.X);
            writer.Write(light.Y);
            writer.Write(light.Z);
            writer.Write(light.Colour);
            writer.Write((byte)light.Type);
            writer.Write(light.Intensity);
            writer.Write(light.Inner);
            writer.Write(light.Outer);
            writer.Write(light.Length);
            writer.Write(light.CutOff);
            writer.Write(light.Direction);
        }
    }

    protected override void WriteStatics(TR4Room room, TRLevelWriter writer)
    {
        writer.Write((ushort)room.StaticMeshes.Count);
        foreach (TR4RoomStaticMesh mesh in room.StaticMeshes)
        {
            writer.Write(mesh.X);
            writer.Write(mesh.Y);
            writer.Write(mesh.Z);
            writer.Write(mesh.Angle);
            writer.Write(mesh.Colour);
            writer.Write(mesh.Unused);
            writer.Write((ushort)(mesh.ID - TR4Type.SceneryBase));
        }
    }

    protected override void WriteAdditionalProperties(TR4Room room, TRLevelWriter writer)
    {
        writer.Write(room.WaterScheme);
        writer.Write((byte)room.ReverbMode);
        writer.Write(room.AlternateGroup);
    }
}
