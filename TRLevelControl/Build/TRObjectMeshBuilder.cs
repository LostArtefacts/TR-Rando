using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl.Build;

public class TRObjectMeshBuilder : IMeshProvider
{
    private readonly TRGameVersion _version;
    private readonly ITRLevelObserver _observer;

    private uint[] _meshPointers;
    private Dictionary<long, TRMesh> _objectMeshes;
    private Dictionary<uint, ushort> _staticMeshPointers;

    public TRMesh GetObjectMesh(long pointer)
        => _objectMeshes[_meshPointers[pointer]];

    public TRObjectMeshBuilder(TRGameVersion version, ITRLevelObserver observer = null)
    {
        _version = version;
        _observer = observer;
    }

    public void BuildObjectMeshes(TRLevelReader reader)
    {
        uint numMeshData = reader.ReadUInt32();
        byte[] meshData = reader.ReadBytes((int)numMeshData * sizeof(short));

        uint numMeshPointers = reader.ReadUInt32();
        _meshPointers = reader.ReadUInt32s(numMeshPointers);

        // The mesh pointer list can contain duplicates so we must make
        // sure to iterate over distinct values only
        uint[] pointers = _meshPointers.Distinct().ToArray();

        _objectMeshes = new();

        using MemoryStream ms = new(meshData);
        using TRLevelReader meshReader = new(ms);

        for (int i = 0; i < pointers.Length; i++)
        {
            uint meshPointer = pointers[i];
            meshReader.BaseStream.Position = meshPointer;

            TRMesh mesh = new();
            _objectMeshes[meshPointer] = mesh;

            mesh.Centre = TR2FileReadUtilities.ReadVertex(meshReader);
            mesh.CollRadius = meshReader.ReadInt32();

            short numVertices = meshReader.ReadInt16();
            mesh.Vertices = new();
            for (int j = 0; j < numVertices; j++)
            {
                mesh.Vertices.Add(TR2FileReadUtilities.ReadVertex(meshReader));
            }

            short numNormals = meshReader.ReadInt16();
            if (numNormals > 0)
            {
                mesh.Normals = new();
                for (int j = 0; j < numNormals; j++)
                {
                    mesh.Normals.Add(TR2FileReadUtilities.ReadVertex(meshReader));
                }
            }
            else
            {
                mesh.Lights = meshReader.ReadInt16s(Math.Abs(numNormals)).ToList();
            }

            short numFaces = meshReader.ReadInt16();
            mesh.TexturedRectangles = meshReader.ReadMeshFaces(numFaces, TRFaceType.Rectangle, _version);

            numFaces = meshReader.ReadInt16();
            mesh.TexturedTriangles = meshReader.ReadMeshFaces(numFaces, TRFaceType.Triangle, _version);

            if (_version < TRGameVersion.TR4)
            {
                numFaces = meshReader.ReadInt16();
                mesh.ColouredRectangles = meshReader.ReadMeshFaces(numFaces, TRFaceType.Rectangle, _version);

                numFaces = meshReader.ReadInt16();
                mesh.ColouredTriangles = meshReader.ReadMeshFaces(numFaces, TRFaceType.Triangle, _version);
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

    public void WriteObjectMeshes(TRLevelWriter writer, IEnumerable<TRMesh> objectMeshes, List<TRStaticMesh> staticMeshes)
    {
        List<TRMesh> cachedMeshes = new();
        List<uint> meshPointers = new();
        List<byte> meshData = new();
        _staticMeshPointers = new();

        void StoreMesh(TRMesh m)
        {
            int index = cachedMeshes.IndexOf(m);
            if (index == -1)
            {
                cachedMeshes.Add(m);
                uint pointer = (uint)meshData.Count;
                meshPointers.Add(pointer);
                meshData.AddRange(Serialize(m, pointer));
            }
            else
            {
                meshPointers.Add(meshPointers[index]);
            }
        }

        foreach (TRMesh mesh in objectMeshes)
        {
            StoreMesh(mesh);
        }

        foreach (TRStaticMesh staticMesh in staticMeshes)
        {
            _staticMeshPointers[staticMesh.ID] = (ushort)meshPointers.Count;
            StoreMesh(staticMesh.Mesh);
        }

        writer.Write((uint)meshData.Count / sizeof(short));
        writer.Write(meshData);

        writer.Write((uint)meshPointers.Count);
        writer.Write(meshPointers);
    }

    public List<TRStaticMesh> ReadStaticMeshes(TRLevelReader reader)
    {
        uint numMeshes = reader.ReadUInt32();
        List<TRStaticMesh> meshes = new();

        for (int i = 0; i < numMeshes; i++)
        {
            meshes.Add(new()
            {
                ID = reader.ReadUInt32(),
                Mesh = GetObjectMesh(reader.ReadUInt16()),
                VisibilityBox = reader.ReadBoundingBox(),
                CollisionBox = reader.ReadBoundingBox(),
                Flags = reader.ReadUInt16()
            });
        }

        return meshes;
    }

    public void WriteStaticMeshes(TRLevelWriter writer, List<TRStaticMesh> staticMeshes)
    {
        writer.Write((uint)staticMeshes.Count);
        foreach (TRStaticMesh staticMesh in staticMeshes)
        {
            writer.Write(staticMesh.ID);
            writer.Write(_staticMeshPointers[staticMesh.ID]);
            writer.Write(staticMesh.VisibilityBox);
            writer.Write(staticMesh.CollisionBox);
            writer.Write(staticMesh.Flags);
        }
    }

    // The pointer here is only required if original mesh padding has been observed.
    public byte[] Serialize(TRMesh mesh, uint meshPointer = 0)
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
        writer.Write(mesh.TexturedRectangles, _version);

        writer.Write((short)mesh.TexturedTriangles.Count);
        writer.Write(mesh.TexturedTriangles, _version);

        if (_version < TRGameVersion.TR4)
        {
            writer.Write((short)mesh.ColouredRectangles.Count);
            writer.Write(mesh.ColouredRectangles, _version);

            writer.Write((short)mesh.ColouredTriangles.Count);
            writer.Write(mesh.ColouredTriangles, _version);
        }

        // 4-byte alignment for mesh data
        int paddingSize = (int)writer.BaseStream.Position % 4;
        if (paddingSize != 0)
        {
            IEnumerable<byte> padding = _observer?.GetMeshPadding(meshPointer) ??
                Enumerable.Repeat((byte)0, paddingSize);
            writer.Write(padding);
        }

        return stream.ToArray();
    }
}
