using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TR1RoomBuilder : TRRoomBuilder<TR1Type, TR1Room>
{
    public TR1RoomBuilder()
        : base(TRGameVersion.TR1) { }

    protected override void ReadLights(TR1Room room, TRLevelReader reader)
    {
        room.AmbientIntensity = reader.ReadInt16();

        ushort numLights = reader.ReadUInt16();
        room.Lights = new();
        for (int j = 0; j < numLights; j++)
        {
            room.Lights.Add(new()
            {
                X = reader.ReadInt32(),
                Y = reader.ReadInt32(),
                Z = reader.ReadInt32(),
                Intensity = reader.ReadUInt16(),
                Fade = reader.ReadUInt32(),
            });
        }
    }

    protected override void ReadStatics(TR1Room room, TRLevelReader reader)
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
                Intensity = reader.ReadUInt16(),
                ID = TR1Type.SceneryBase + reader.ReadUInt16()
            });
        }
    }

    protected override void BuildMesh(TR1Room room, TRLevelReader reader, ISpriteProvider<TR1Type> spriteProvider)
    {
        short numVertices = reader.ReadInt16();
        List<TR1RoomVertex> vertices = new();
        for (int i = 0; i < numVertices; i++)
        {
            vertices.Add(new()
            {
                Vertex = reader.ReadVertex(),
                Lighting = reader.ReadInt16(),
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

    protected override void WriteMesh(TR1Room room, TRLevelWriter writer, ISpriteProvider<TR1Type> spriteProvider)
    {
        writer.Write((short)room.Mesh.Vertices.Count);
        foreach (TR1RoomVertex vertex in room.Mesh.Vertices)
        {
            writer.Write(vertex.Vertex);
            writer.Write(vertex.Lighting);
        }

        WriteFaces(room.Mesh.Rectangles, writer);
        WriteFaces(room.Mesh.Triangles, writer);
        WriteSprites(room.Mesh.Sprites, spriteProvider, writer);
    }

    protected override void WriteLights(TR1Room room, TRLevelWriter writer)
    {
        writer.Write(room.AmbientIntensity);

        writer.Write((ushort)room.Lights.Count);
        foreach (TR1RoomLight light in room.Lights)
        {
            writer.Write(light.X);
            writer.Write(light.Y);
            writer.Write(light.Z);
            writer.Write(light.Intensity);
            writer.Write(light.Fade);
        }
    }

    protected override void WriteStatics(TR1Room room, TRLevelWriter writer)
    {
        writer.Write((ushort)room.StaticMeshes.Count);
        foreach (TR1RoomStaticMesh mesh in room.StaticMeshes)
        {
            writer.Write(mesh.X);
            writer.Write(mesh.Y);
            writer.Write(mesh.Z);
            writer.Write(mesh.Angle);
            writer.Write(mesh.Intensity);
            writer.Write((ushort)(mesh.ID - TR1Type.SceneryBase));
        }
    }
}
