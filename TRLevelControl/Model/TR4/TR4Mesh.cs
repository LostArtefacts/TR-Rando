using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TRLevelControl.Serialization;

namespace TRLevelControl.Model;

public class TR4Mesh : ISerializableCompact
{
    //Held for convenience here but is not included in serialisation
    //Value matches that in containing pointer array
    public uint Pointer { get; set; }

    public TRVertex Centre { get; set; }

    public int CollRadius { get; set; }

    public short NumVertices { get; set; }

    public TRVertex[] Vertices { get; set; }

    public short NumNormals { get; set; }

    public TRVertex[] Normals { get; set; }

    public short[] Lights { get; set; }

    public short NumTexturedRectangles { get; set; }

    public TR4MeshFace4[] TexturedRectangles { get; set; }

    public short NumTexturedTriangles { get; set; }

    public TR4MeshFace3[] TexturedTriangles { get; set; }

    public byte[] Serialize()
    {
        using MemoryStream stream = new();
        using (BinaryWriter writer = new(stream))
        {
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
            foreach (TR4MeshFace4 face in TexturedRectangles)
            {
                writer.Write(face.Serialize());
            }

            writer.Write(NumTexturedTriangles);
            foreach (TR4MeshFace3 face in TexturedTriangles)
            {
                writer.Write(face.Serialize());
            }

            // 4-byte alignment for mesh data
            long padding = writer.BaseStream.Position % 4;
            for (int i = 0; i < padding; i++)
            {
                writer.Write((byte)0);
            }
        }

        return stream.ToArray();
    }
}
