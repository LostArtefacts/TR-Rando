using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TR2RoomBuilder : TRRoomBuilder<TR2Type, TR2Room>
{
    public TR2RoomBuilder()
        : base(TRGameVersion.TR2) { }

    protected override void ReadLights(TR2Room room, TRLevelReader reader)
    {
        room.AmbientIntensity = reader.ReadInt16();
        room.AmbientIntensity2 = reader.ReadInt16();
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
                Intensity1 = reader.ReadUInt16(),
                Intensity2 = reader.ReadUInt16(),
                Fade1 = reader.ReadUInt32(),
                Fade2 = reader.ReadUInt32()
            });
        }
    }

    protected override void ReadStatics(TR2Room room, TRLevelReader reader)
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
                Intensity1 = reader.ReadUInt16(),
                Intensity2 = reader.ReadUInt16(),
                ID = TR2Type.SceneryBase + reader.ReadUInt16()
            });
        }
    }

    protected override void BuildMesh(TR2Room room, TRLevelReader reader, ISpriteProvider<TR2Type> spriteProvider)
    {
        short numVertices = reader.ReadInt16();
        List<TR2RoomVertex> vertices = new();
        for (int i = 0; i < numVertices; i++)
        {
            vertices.Add(new()
            {
                Vertex = reader.ReadVertex(),
                Lighting = reader.ReadInt16(),
                Attributes = reader.ReadUInt16(),
                Lighting2 = reader.ReadInt16(),
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

    protected override void WriteMesh(TR2Room room, TRLevelWriter writer, ISpriteProvider<TR2Type> spriteProvider)
    {
        writer.Write((short)room.Mesh.Vertices.Count);
        foreach (TR2RoomVertex vertex in room.Mesh.Vertices)
        {
            writer.Write(vertex.Vertex);
            writer.Write(vertex.Lighting);
            writer.Write(vertex.Attributes);
            writer.Write(vertex.Lighting2);
        }

        WriteFaces(room.Mesh.Rectangles, writer);
        WriteFaces(room.Mesh.Triangles, writer);
        WriteSprites(room.Mesh.Sprites, spriteProvider, writer);
    }

    protected override void WriteLights(TR2Room room, TRLevelWriter writer)
    {
        writer.Write(room.AmbientIntensity);
        writer.Write(room.AmbientIntensity2);
        writer.Write(room.LightMode);

        writer.Write((ushort)room.Lights.Count);
        foreach (TR2RoomLight light in room.Lights)
        {
            writer.Write(light.X);
            writer.Write(light.Y);
            writer.Write(light.Z);
            writer.Write(light.Intensity1);
            writer.Write(light.Intensity2);
            writer.Write(light.Fade1);
            writer.Write(light.Fade2);
        }
    }

    protected override void WriteStatics(TR2Room room, TRLevelWriter writer)
    {
        writer.Write((ushort)room.StaticMeshes.Count);
        foreach (TR2RoomStaticMesh mesh in room.StaticMeshes)
        {
            writer.Write(mesh.X);
            writer.Write(mesh.Y);
            writer.Write(mesh.Z);
            writer.Write(mesh.Angle);
            writer.Write(mesh.Intensity1);
            writer.Write(mesh.Intensity2);
            writer.Write((ushort)(mesh.ID - TR2Type.SceneryBase));
        }
    }
}
