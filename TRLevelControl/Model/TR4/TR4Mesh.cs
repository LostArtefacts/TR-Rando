namespace TRLevelControl.Model;

public class TR4Mesh
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
}
