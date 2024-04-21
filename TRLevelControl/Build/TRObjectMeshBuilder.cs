using System;
using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRObjectMeshBuilder
{
    private readonly ITRLevelObserver _observer;

    public List<TR4Mesh> Meshes { get; private set; }
    public List<uint> MeshPointers { get; private set; }

    public TRObjectMeshBuilder(ITRLevelObserver observer = null)
    {
        _observer = observer;
    }

    public void BuildObjectMeshes(TRLevelReader reader)
    {
        uint numMeshData = reader.ReadUInt32();
        byte[] meshData = reader.ReadBytes((int)numMeshData * sizeof(short));

        uint numMeshPointers = reader.ReadUInt32();
        MeshPointers = new(reader.ReadUInt32s(numMeshPointers));

        // The mesh pointer list can contain duplicates so we must make
        // sure to iterate over distinct values only
        uint[] pointers = MeshPointers.Distinct().ToArray();

        Meshes = new();

        using MemoryStream ms = new(meshData);
        using TRLevelReader meshReader = new(ms);

        for (int i = 0; i < pointers.Length; i++)
        {
            uint meshPointer = pointers[i];
            meshReader.BaseStream.Position = meshPointer;

            TR4Mesh mesh = new();
            Meshes.Add(mesh);

            mesh.Pointer = meshPointer;

            mesh.Centre = TR2FileReadUtilities.ReadVertex(meshReader);
            mesh.CollRadius = meshReader.ReadInt32();

            mesh.NumVertices = meshReader.ReadInt16();
            mesh.Vertices = new TRVertex[mesh.NumVertices];
            for (int j = 0; j < mesh.NumVertices; j++)
            {
                mesh.Vertices[j] = TR2FileReadUtilities.ReadVertex(meshReader);
            }

            mesh.NumNormals = meshReader.ReadInt16();
            if (mesh.NumNormals > 0)
            {
                mesh.Normals = new TRVertex[mesh.NumNormals];
                for (int j = 0; j < mesh.NumNormals; j++)
                {
                    mesh.Normals[j] = TR2FileReadUtilities.ReadVertex(meshReader);
                }
            }
            else
            {
                mesh.Lights = new short[Math.Abs(mesh.NumNormals)];
                for (int j = 0; j < mesh.Lights.Length; j++)
                {
                    mesh.Lights[j] = meshReader.ReadInt16();
                }
            }

            mesh.NumTexturedRectangles = meshReader.ReadInt16();
            mesh.TexturedRectangles = new TR4MeshFace4[mesh.NumTexturedRectangles];
            for (int j = 0; j < mesh.NumTexturedRectangles; j++)
            {
                mesh.TexturedRectangles[j] = TR4FileReadUtilities.ReadTR4MeshFace4(meshReader);
            }

            mesh.NumTexturedTriangles = meshReader.ReadInt16();
            mesh.TexturedTriangles = new TR4MeshFace3[mesh.NumTexturedTriangles];
            for (int j = 0; j < mesh.NumTexturedTriangles; j++)
            {
                mesh.TexturedTriangles[j] = TR4FileReadUtilities.ReadTR4MeshFace3(meshReader);
            }

            // Padding - TR1-3 use 0s, TR4 and 5 use random values. This allows tests to observe and restore.
            if (_observer != null && meshReader.BaseStream.Position % 4 != 0)
            {
                long nextPointer = i < pointers.Length - 1 ? pointers[i + 1] : meshReader.BaseStream.Length;
                long paddingSize = nextPointer - meshReader.BaseStream.Position;
                _observer.OnMeshPaddingRead(meshPointer, meshReader.ReadBytes((int)paddingSize).ToList());
            }
        }
    }

    public byte[] Serialize(TR4Mesh mesh)
    {
        using MemoryStream stream = new();
        TRLevelWriter writer = new(stream);

        writer.Write(mesh.Centre.Serialize());
        writer.Write(mesh.CollRadius);

        writer.Write(mesh.NumVertices);
        foreach (TRVertex vert in mesh.Vertices)
        {
            writer.Write(vert.Serialize());
        }

        writer.Write(mesh.NumNormals);
        if (mesh.NumNormals > 0)
        {
            foreach (TRVertex normal in mesh.Normals)
            {
                writer.Write(normal.Serialize());
            }
        }
        else
        {
            foreach (short light in mesh.Lights)
            {
                writer.Write(light);
            }
        }

        writer.Write(mesh.NumTexturedRectangles);
        foreach (TR4MeshFace4 face in mesh.TexturedRectangles)
        {
            writer.Write(face.Serialize());
        }

        writer.Write(mesh.NumTexturedTriangles);
        foreach (TR4MeshFace3 face in mesh.TexturedTriangles)
        {
            writer.Write(face.Serialize());
        }

        // 4-byte alignment for mesh data
        int paddingSize = (int)writer.BaseStream.Position % 4;
        if (paddingSize != 0)
        {
            IEnumerable<byte> padding = _observer?.GetMeshPadding(mesh.Pointer) ??
                Enumerable.Repeat((byte)0, paddingSize);
            writer.Write(padding.ToArray());
        }

        return stream.ToArray();
    }

    // These methods are temporary until we merge TR1-5 meshes
    public static List<TRMesh> ConstructMeshData(List<uint> meshPointers, ushort[] rawMeshData)
    {
        byte[] target = new byte[rawMeshData.Length * 2];
        Buffer.BlockCopy(rawMeshData, 0, target, 0, target.Length);

        meshPointers = new(meshPointers.Distinct());

        List<TRMesh> meshes = new();

        using MemoryStream ms = new(target);
        using TRLevelReader br = new(ms);

        for (int i = 0; i < meshPointers.Count; i++)
        {
            TRMesh mesh = new();
            meshes.Add(mesh);

            uint meshPointer = meshPointers[i];
            br.BaseStream.Position = meshPointer;

            mesh.Pointer = meshPointer;
            mesh.Centre = TR2FileReadUtilities.ReadVertex(br);
            mesh.CollRadius = br.ReadInt32();

            short numVertices = br.ReadInt16();
            mesh.Vertices = new();
            for (int j = 0; j < numVertices; j++)
            {
                mesh.Vertices.Add(TR2FileReadUtilities.ReadVertex(br));
            }

            short numNormals = br.ReadInt16();
            if (numNormals > 0)
            {
                mesh.Normals = new();
                for (int j = 0; j < numNormals; j++)
                {
                    mesh.Normals.Add(TR2FileReadUtilities.ReadVertex(br));
                }
            }
            else
            {
                mesh.Lights = br.ReadInt16s(Math.Abs(numNormals)).ToList();
            }

            short numFaces = br.ReadInt16();
            mesh.TexturedRectangles = new();
            for (int j = 0; j < numFaces; j++)
            {
                mesh.TexturedRectangles.Add(TR2FileReadUtilities.ReadTRFace4(br));
            }

            numFaces = br.ReadInt16();
            mesh.TexturedTriangles = new();
            for (int j = 0; j < numFaces; j++)
            {
                mesh.TexturedTriangles.Add(TR2FileReadUtilities.ReadTRFace3(br));
            }

            numFaces = br.ReadInt16();
            mesh.ColouredRectangles = new();
            for (int j = 0; j < numFaces; j++)
            {
                mesh.ColouredRectangles.Add(TR2FileReadUtilities.ReadTRFace4(br));
            }

            numFaces = br.ReadInt16();
            mesh.ColouredTriangles = new();
            for (int j = 0; j < numFaces; j++)
            {
                mesh.ColouredTriangles.Add(TR2FileReadUtilities.ReadTRFace3(br));
            }
        }

        return meshes;
    }

    public static byte[] Serialize(TRMesh mesh)
    {
        using MemoryStream stream = new();
        TRLevelWriter writer = new(stream);

        writer.Write(mesh.Centre.Serialize());
        writer.Write(mesh.CollRadius);

        writer.Write((short)mesh.Vertices.Count);
        foreach (TRVertex vert in mesh.Vertices)
        {
            writer.Write(vert.Serialize());
        }

        Debug.Assert(mesh.Normals == null ^ mesh.Lights == null);
        if (mesh.Normals != null)
        {
            writer.Write((short)mesh.Normals.Count);
            foreach (TRVertex normal in mesh.Normals)
            {
                writer.Write(normal.Serialize());
            }
        }
        else
        {
            writer.Write((short)-mesh.Lights.Count);
            writer.Write(mesh.Lights);
        }

        writer.Write((short)mesh.TexturedRectangles.Count);
        foreach (TRFace4 face in mesh.TexturedRectangles)
        {
            writer.Write(face.Serialize());
        }

        writer.Write((short)mesh.TexturedTriangles.Count);
        foreach (TRFace3 face in mesh.TexturedTriangles)
        {
            writer.Write(face.Serialize());
        }

        writer.Write((short)mesh.ColouredRectangles.Count);
        foreach (TRFace4 face in mesh.ColouredRectangles)
        {
            writer.Write(face.Serialize());
        }

        writer.Write((short)mesh.ColouredTriangles.Count);
        foreach (TRFace3 face in mesh.ColouredTriangles)
        {
            writer.Write(face.Serialize());
        }

        // 4-byte alignment for mesh data
        int paddingSize = (int)writer.BaseStream.Position % 4;
        writer.Write(Enumerable.Repeat((byte)0, paddingSize));

        return stream.ToArray();
    }
}
