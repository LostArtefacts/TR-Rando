using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TRGControlBase
{
    private const uint _trgMagic = 'T' | ('R' << 8) | ('G' << 16) | (3 << 24);

    public static TRGData Read(string filePath)
        => Read(File.OpenRead(filePath));

    public static TRGData Read(Stream stream)
    {
        using TRLevelReader reader = new(stream);
        uint magic = reader.ReadUInt32();
        Debug.Assert(magic == _trgMagic);

        TRGData data = new();

        uint numAmbientCubes = reader.ReadUInt32();
        data.AmbientCubes = new();
        for (int i = 0; i < numAmbientCubes; i++)
        {
            TRColour4[] cube = new TRColour4[6];
            data.AmbientCubes.Add(cube);
            for (int j = 0; j < cube.Length; j++)
            {
                cube[j] = new(reader.ReadUInt32());
            }
        }

        data.Unknown1 = reader.ReadBytes(24);
        data.Unknown2 = reader.ReadByte();
        data.Unknown3 = reader.ReadByte();

        byte numUnknown4 = reader.ReadByte();
        byte numUnknown5 = reader.ReadByte();

        data.Unknown4 = new();
        for (int i = 0; i < numUnknown4; i++)
        {
            data.Unknown4.Add(reader.ReadBytes(40));
        }

        data.Unknown5 = new();
        for (int i = 0; i < numUnknown5; i++)
        {
            data.Unknown5.Add(reader.ReadBytes(28));
        }

        uint numMeshes = reader.ReadUInt32();
        data.Meshes = new();
        for (int i = 0; i < numMeshes; i++)
        {
            TRGMesh mesh = new();
            data.Meshes.Add(mesh);

            uint numShaders = reader.ReadUInt32();
            mesh.Shaders = new();
            for (int j = 0; j < numShaders; j++)
            {
                TRGShader shader = new()
                {
                    Type = reader.ReadUInt32(),
                    Unknown1 = reader.ReadUInt32(),
                    Unknown2 = reader.ReadUInt32(),
                    Unknown3 = reader.ReadUInt32(),
                    Unknown4 = reader.ReadUInt32(),
                    IndexInfo = new Tuple<uint, uint>[3]
                };
                mesh.Shaders.Add(shader);

                for (int k = 0; k < 3; k++)
                {
                    shader.IndexInfo[k] = new(reader.ReadUInt32(), reader.ReadUInt32());
                }
            }

            mesh.Unknown1 = reader.ReadUInt32();
            uint numUnknowns = reader.ReadUInt32();
            mesh.Unknown2 = new();
            for (int j = 0; j < numUnknowns; j++)
            {
                mesh.Unknown2.Add(reader.ReadUInt8s(28));
            }
        }

        uint numTextures = reader.ReadUInt32();
        data.Textures = new(reader.ReadUInt16s(numTextures));

        while (reader.BaseStream.Position % sizeof(uint) != 0)
        {
            reader.ReadByte();
        }

        uint numIndices = reader.ReadUInt32();
        uint numVertices = reader.ReadUInt32();
        data.Indices = new(reader.ReadUInt32s(numIndices));
        data.Vertices = new();
        for (int i = 0; i < numVertices; i++)
        {
            data.Vertices.Add(new()
            {
                Vertex = reader.ReadVertex(),
                Unknown = reader.ReadInt16(),
                Normal = reader.ReadVertex8(),
                Texture = reader.ReadByte(),
                Ambient1 = reader.ReadColour(),
                U = reader.ReadByte(),
                Ambient2 = reader.ReadColour(),
                V = reader.ReadByte(),
            });
        }

        Debug.Assert(reader.BaseStream.Position ==  reader.BaseStream.Length);

        return data;
    }

    public static void Write(TRGData data, string filePath)
        => Write(data, File.Create(filePath));

    public static void Write(TRGData data, Stream outputStream)
    {
        using TRLevelWriter writer = new(outputStream);
        writer.Write(_trgMagic);

        writer.Write((uint)data.AmbientCubes.Count);
        writer.Write(data.AmbientCubes.SelectMany(c => c.Select(a => a.ToARGB())));

        writer.Write(data.Unknown1);
        writer.Write(data.Unknown2);
        writer.Write(data.Unknown3);

        writer.Write((byte)data.Unknown4.Count);
        writer.Write((byte)data.Unknown5.Count);
        writer.Write(data.Unknown4.SelectMany(u => u));
        writer.Write(data.Unknown5.SelectMany(u => u));

        writer.Write((uint)data.Meshes.Count);
        foreach (TRGMesh mesh in data.Meshes)
        {
            writer.Write((uint)mesh.Shaders.Count);
            foreach (TRGShader shader in mesh.Shaders)
            {
                writer.Write(shader.Type);
                writer.Write(shader.Unknown1);
                writer.Write(shader.Unknown2);
                writer.Write(shader.Unknown3);
                writer.Write(shader.Unknown4);
                foreach (var (index, offset) in shader.IndexInfo)
                {
                    writer.Write(index);
                    writer.Write(offset);
                }
            }

            writer.Write(mesh.Unknown1);
            writer.Write((uint)mesh.Unknown2.Count);
            writer.Write(mesh.Unknown2.SelectMany(u => u));
        }

        writer.Write((uint)data.Textures.Count);
        writer.Write(data.Textures);

        while (writer.BaseStream.Position % sizeof(uint) != 0)
        {
            writer.Write((byte)0);
        }

        writer.Write((uint)data.Indices.Count);
        writer.Write((uint)data.Vertices.Count);
        writer.Write(data.Indices);

        foreach (TRGVertex vertex in data.Vertices)
        {
            writer.Write(vertex.Vertex);
            writer.Write(vertex.Unknown);
            writer.Write(vertex.Normal);
            writer.Write(vertex.Texture);
            writer.Write(vertex.Ambient1);
            writer.Write(vertex.U);
            writer.Write(vertex.Ambient2);
            writer.Write(vertex.V);
        }
    }
}
