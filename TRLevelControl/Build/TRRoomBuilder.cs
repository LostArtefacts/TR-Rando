using TRLevelControl.Model;

namespace TRLevelControl.Build;

public abstract class TRRoomBuilder<T, V>
    where T : Enum
    where V : TRRoomVertex
{
    protected readonly TRGameVersion _version;
    protected List<ushort[]> _rawMeshes = new();

    public TRRoomBuilder(TRGameVersion version)
    {
        _version = version;
    }

    public void ReadRawMesh(TRLevelReader reader)
    {
        uint numMeshData = reader.ReadUInt32();
        _rawMeshes.Add(reader.ReadUInt16s(numMeshData));
    }

    protected abstract List<V> ReadVertices(TRLevelReader reader);
    protected abstract void WriteVertices(TRLevelWriter writer, List<V> vertices);

    public TRRoomMesh<T, V> BuildMesh(int roomIndex, ISpriteProvider<T> spriteProvider)
    {
        ushort[] rawData = _rawMeshes[roomIndex];
        byte[] target = new byte[rawData.Length * sizeof(short)];
        Buffer.BlockCopy(rawData, 0, target, 0, target.Length);

        using MemoryStream ms = new(target);
        using TRLevelReader reader = new(ms);

        TRRoomMesh<T, V> mesh = new()
        {
            Vertices = ReadVertices(reader)
        };

        short numFaces = reader.ReadInt16();
        mesh.Rectangles = new();
        for (int i = 0; i < numFaces; i++)
        {
            mesh.Rectangles.Add(new()
            {
                Vertices = reader.ReadUInt16s(4),
                Texture = reader.ReadUInt16(),
            });
        }

        numFaces = reader.ReadInt16();
        mesh.Triangles = new();
        for (int i = 0; i < numFaces; i++)
        {
            mesh.Triangles.Add(new()
            {
                Vertices = reader.ReadUInt16s(3),
                Texture = reader.ReadUInt16(),
            });
        }

        short numSprites = reader.ReadInt16();
        mesh.Sprites = new();

        for (int i = 0; i < numSprites; i++)
        {
            mesh.Sprites.Add(new()
            {
                Vertex = reader.ReadInt16(),
                ID = spriteProvider.FindSpriteType(reader.ReadInt16())
            });
        }

        return mesh;
    }

    public void WriteMesh(TRLevelWriter writer, TRRoomMesh<T, V> mesh, ISpriteProvider<T> spriteProvider)
    {
        using MemoryStream stream = new();
        using TRLevelWriter meshWriter = new(stream);

        WriteVertices(meshWriter, mesh.Vertices);

        meshWriter.Write((short)mesh.Rectangles.Count);
        foreach (TRFace4 face in mesh.Rectangles)
        {
            meshWriter.Write(face.Vertices);
            meshWriter.Write(face.Texture);
        }

        meshWriter.Write((short)mesh.Triangles.Count);
        foreach (TRFace3 face in mesh.Triangles)
        {
            meshWriter.Write(face.Vertices);
            meshWriter.Write(face.Texture);
        }

        meshWriter.Write((short)mesh.Sprites.Count);
        foreach (TRRoomSprite<T> sprite in mesh.Sprites)
        {
            meshWriter.Write(sprite.Vertex);
            meshWriter.Write(spriteProvider.GetSpriteOffset(sprite.ID));
        }

        byte[] data = stream.ToArray();
        writer.Write((uint)data.Length / sizeof(short));
        writer.Write(data);
    }
}
