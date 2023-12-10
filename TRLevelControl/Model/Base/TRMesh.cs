using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TRMesh : ISerializableCompact, ICloneable
{
    //Held for convenience here but is not included in serialisation
    //Value matches that in containing pointer array
    public uint Pointer { get; set; }

    //6 Bytes
    public TRVertex Centre { get; set; }

    //4 Bytes
    public int CollRadius { get; set; }

    //2 Bytes
    public short NumVertices { get; set; }

    //NumVertices * 6 Bytes
    public TRVertex[] Vertices { get; set; }

    //2 bytes - if this is negative, lights is populated. Otherwise Normals populated
    public short NumNormals { get; set; }

    //NumNormals * 6 bytes OR
    public TRVertex[] Normals { get; set; }

    //NumNormals * 2 bytes - It's either Normals (+) or Lights (-) not both
    public short[] Lights { get; set; }

    //2 bytes
    public short NumTexturedRectangles { get; set; }

    //NumTexturedRectangles * 10 bytes
    public TRFace4[] TexturedRectangles { get; set; }

    //2 bytes
    public short NumTexturedTriangles { get; set; }

    //NumTexturedTriangles * 8 bytes
    public TRFace3[] TexturedTriangles { get; set; }

    //2 bytes
    public short NumColouredRectangles { get; set; }

    //NumColouredRectangles * 10 bytes
    public TRFace4[] ColouredRectangles { get; set; }

    //2 bytes
    public short NumColouredTriangles { get; set; }

    //NumColouredTriangles * 8 bytes
    public TRFace3[] ColouredTriangles { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write(Centre.Serialize());
        writer.Write(CollRadius);
        writer.Write(NumVertices);

        foreach (TRVertex vert in Vertices)
        {
            writer.Write(vert.Serialize());
        }

        writer.Write(NumNormals);

        if (NumNormals > 0)
        {
            foreach (TRVertex normal in Normals)
            {
                writer.Write(normal.Serialize());
            }
        }
        else
        {
            foreach (short light in Lights)
            {
                writer.Write(light);
            }
        }

        writer.Write(NumTexturedRectangles);
        foreach (TRFace4 face in TexturedRectangles)
        {
            writer.Write(face.Serialize());
        }

        writer.Write(NumTexturedTriangles);
        foreach (TRFace3 face in TexturedTriangles)
        {
            writer.Write(face.Serialize());
        }

        writer.Write(NumColouredRectangles);
        foreach (TRFace4 face in ColouredRectangles)
        {
            writer.Write(face.Serialize());
        }

        writer.Write(NumColouredTriangles);
        foreach (TRFace3 face in ColouredTriangles)
        {
            writer.Write(face.Serialize());
        }

        // 4-byte alignment for mesh data
        long padding = writer.BaseStream.Position % 4;
        for (int i = 0; i < padding; i++)
        {
            writer.Write((byte)0);
        }

        return stream.ToArray();
    }

    public TRMesh Clone()
    {
        TRMesh clone = new()
        {
            Centre = Centre,
            CollRadius = CollRadius,
            ColouredRectangles = new TRFace4[NumColouredRectangles],
            ColouredTriangles = new TRFace3[NumColouredTriangles],
            Lights = Lights,
            Normals = Normals,
            NumColouredRectangles = NumColouredRectangles,
            NumColouredTriangles = NumColouredTriangles,
            NumNormals = NumNormals,
            NumTexturedRectangles = NumTexturedRectangles,
            NumTexturedTriangles = NumTexturedTriangles,
            NumVertices = NumVertices,
            Pointer = Pointer,
            TexturedRectangles = new TRFace4[NumTexturedRectangles],
            TexturedTriangles = new TRFace3[NumTexturedTriangles],
            Vertices = new TRVertex[NumVertices]
        };

        for (int i = 0; i < NumColouredRectangles; i++)
        {
            clone.ColouredRectangles[i] = new()
            {
                Texture = ColouredRectangles[i].Texture,
                Vertices = new List<ushort>(ColouredRectangles[i].Vertices).ToArray()
            };
        }

        for (int i = 0; i < NumColouredTriangles; i++)
        {
            clone.ColouredTriangles[i] = new()
            {
                Texture = ColouredTriangles[i].Texture,
                Vertices = new List<ushort>(ColouredTriangles[i].Vertices).ToArray()
            };
        }

        for (int i = 0; i < NumTexturedRectangles; i++)
        {
            clone.TexturedRectangles[i] = new()
            {
                Texture = TexturedRectangles[i].Texture,
                Vertices = new List<ushort>(TexturedRectangles[i].Vertices).ToArray()
            };
        }

        for (int i = 0; i < NumTexturedTriangles; i++)
        {
            clone.TexturedTriangles[i] = new()
            {
                Texture = TexturedTriangles[i].Texture,
                Vertices = new List<ushort>(TexturedTriangles[i].Vertices).ToArray()
            };
        }

        for (int i = 0; i < NumVertices; i++)
        {
            clone.Vertices[i] = new()
            {
                X = Vertices[i].X,
                Y = Vertices[i].Y,
                Z = Vertices[i].Z
            };
        }

        return clone;
    }

    object ICloneable.Clone()
        => Clone();
}
