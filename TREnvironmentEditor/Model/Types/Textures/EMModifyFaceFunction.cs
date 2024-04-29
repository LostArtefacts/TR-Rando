using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMModifyFaceFunction : BaseEMFunction
{
    public EMFaceModification[] Modifications { get; set; }
    public EMFaceRotation[] Rotations { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        if (Modifications != null)
        {
            foreach (EMFaceModification mod in Modifications)
            {
                TR1Room room = level.Rooms[data.ConvertRoom(mod.RoomNumber)];
                switch (mod.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        ModifyRectangles(room, mod);
                        break;
                    case EMTextureFaceType.Triangles:
                        ModifyTriangles(room, mod);
                        break;
                }
            }
        }

        if (Rotations != null)
        {
            foreach (EMFaceRotation rot in Rotations)
            {
                TR1Room room = level.Rooms[data.ConvertRoom(rot.RoomNumber)];
                switch (rot.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        RotateRectangles(room.RoomData.Rectangles.ToArray(), rot);
                        break;
                    case EMTextureFaceType.Triangles:
                        RotateTriangles(room.RoomData.Triangles.ToArray(), rot);
                        break;
                }
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        if (Modifications != null)
        {
            foreach (EMFaceModification mod in Modifications)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(mod.RoomNumber)];
                switch (mod.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        ModifyRectangles(room, mod);
                        break;
                    case EMTextureFaceType.Triangles:
                        ModifyTriangles(room, mod);
                        break;
                }
            }
        }

        if (Rotations != null)
        {
            foreach (EMFaceRotation rot in Rotations)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(rot.RoomNumber)];
                switch (rot.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        RotateRectangles(room.RoomData.Rectangles, rot);
                        break;
                    case EMTextureFaceType.Triangles:
                        RotateTriangles(room.RoomData.Triangles, rot);
                        break;
                }
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        if (Modifications != null)
        {
            foreach (EMFaceModification mod in Modifications)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(mod.RoomNumber)];
                switch (mod.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        ModifyRectangles(room, mod);
                        break;
                    case EMTextureFaceType.Triangles:
                        ModifyTriangles(room, mod);
                        break;
                }
            }
        }

        if (Rotations != null)
        {
            foreach (EMFaceRotation rot in Rotations)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(rot.RoomNumber)];
                switch (rot.FaceType)
                {
                    case EMTextureFaceType.Rectangles:
                        RotateRectangles(room.RoomData.Rectangles, rot);
                        break;
                    case EMTextureFaceType.Triangles:
                        RotateTriangles(room.RoomData.Triangles, rot);
                        break;
                }
            }
        }
    }

    private static void ModifyRectangles(TR1Room room, EMFaceModification mod)
    {
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace4 rect = room.RoomData.Rectangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR1RoomVertex currentRoomVertex = room.RoomData.Vertices[rect.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR1RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                rect.Vertices[vertIndex] = (ushort)room.RoomData.Vertices.Count;
                room.RoomData.Vertices.Add(newRoomVertex);
            }
        }
    }

    private static void ModifyRectangles(TR2Room room, EMFaceModification mod)
    {
        List<TR2RoomVertex> allVertices = room.RoomData.Vertices.ToList();
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace4 rect = room.RoomData.Rectangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR2RoomVertex currentRoomVertex = allVertices[rect.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR2RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                rect.Vertices[vertIndex] = (ushort)allVertices.Count;
                allVertices.Add(newRoomVertex);
            }
        }

        room.RoomData.Vertices = allVertices.ToArray();
        room.RoomData.NumVertices = (short)allVertices.Count;
        room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
    }

    private static void ModifyRectangles(TR3Room room, EMFaceModification mod)
    {
        List<TR3RoomVertex> allVertices = room.RoomData.Vertices.ToList();
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace4 rect = room.RoomData.Rectangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR3RoomVertex currentRoomVertex = allVertices[rect.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR3RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                rect.Vertices[vertIndex] = (ushort)allVertices.Count;
                allVertices.Add(newRoomVertex);
            }
        }

        room.RoomData.Vertices = allVertices.ToArray();
        room.RoomData.NumVertices = (short)allVertices.Count;
        room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
    }

    private static void ModifyTriangles(TR1Room room, EMFaceModification mod)
    {
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace3 tri = room.RoomData.Triangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR1RoomVertex currentRoomVertex = room.RoomData.Vertices[tri.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR1RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                tri.Vertices[vertIndex] = (ushort)room.RoomData.Vertices.Count;
                room.RoomData.Vertices.Add(newRoomVertex);
            }
        }
    }

    private static void ModifyTriangles(TR2Room room, EMFaceModification mod)
    {
        List<TR2RoomVertex> allVertices = room.RoomData.Vertices.ToList();
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace3 tri = room.RoomData.Triangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR2RoomVertex currentRoomVertex = allVertices[tri.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR2RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                tri.Vertices[vertIndex] = (ushort)allVertices.Count;
                allVertices.Add(newRoomVertex);
            }
        }

        room.RoomData.Vertices = allVertices.ToArray();
        room.RoomData.NumVertices = (short)allVertices.Count;
        room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
    }

    private static void ModifyTriangles(TR3Room room, EMFaceModification mod)
    {
        List<TR3RoomVertex> allVertices = room.RoomData.Vertices.ToList();
        foreach (int faceIndex in mod.GetIndices())
        {
            TRFace3 tri = room.RoomData.Triangles[faceIndex];
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR3RoomVertex currentRoomVertex = allVertices[tri.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR3RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                tri.Vertices[vertIndex] = (ushort)allVertices.Count;
                allVertices.Add(newRoomVertex);
            }
        }

        room.RoomData.Vertices = allVertices.ToArray();
        room.RoomData.NumVertices = (short)allVertices.Count;
        room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
    }

    private static TR1RoomVertex GenerateRoomVertex(TR1RoomVertex currentRoomVertex, TRVertex newVertex)
    {
        // We create a new vertex because we can't guarantee nothing else is using it.
        // Note the vertex values in the mod are added to the existing values, so we
        // don't have to define those we aren't changing.
        return new()
        {
            Lighting = currentRoomVertex.Lighting,
            Vertex = new()
            {
                X = (short)(currentRoomVertex.Vertex.X + newVertex.X),
                Y = (short)(currentRoomVertex.Vertex.Y + newVertex.Y),
                Z = (short)(currentRoomVertex.Vertex.Z + newVertex.Z)
            }
        };
    }

    private static TR2RoomVertex GenerateRoomVertex(TR2RoomVertex currentRoomVertex, TRVertex newVertex)
    {
        return new()
        {
            Attributes = currentRoomVertex.Attributes,
            Lighting = currentRoomVertex.Lighting,
            Lighting2 = currentRoomVertex.Lighting2,
            Vertex = new()
            {
                X = (short)(currentRoomVertex.Vertex.X + newVertex.X),
                Y = (short)(currentRoomVertex.Vertex.Y + newVertex.Y),
                Z = (short)(currentRoomVertex.Vertex.Z + newVertex.Z)
            }
        };
    }

    private static TR3RoomVertex GenerateRoomVertex(TR3RoomVertex currentRoomVertex, TRVertex newVertex)
    {
        return new()
        {
            Attributes = currentRoomVertex.Attributes,
            Lighting = currentRoomVertex.Lighting,
            Colour = currentRoomVertex.Colour,
            Vertex = new()
            {
                X = (short)(currentRoomVertex.Vertex.X + newVertex.X),
                Y = (short)(currentRoomVertex.Vertex.Y + newVertex.Y),
                Z = (short)(currentRoomVertex.Vertex.Z + newVertex.Z)
            }
        };
    }

    private static void RotateRectangles(TRFace4[] rectangles, EMFaceRotation rot)
    {
        foreach (int rectIndex in rot.FaceIndices)
        {
            TRFace4 face = rectangles[rectIndex];
            face.Vertices = RotateVertices(face.Vertices, rot);
        }
    }

    private static void RotateTriangles(TRFace3[] triangles, EMFaceRotation rot)
    {
        foreach (int triIndex in rot.FaceIndices)
        {
            TRFace3 face = triangles[triIndex];
            face.Vertices = RotateVertices(face.Vertices, rot);
        }
    }

    public static ushort[] RotateVertices(ushort[] originalVertices, EMFaceRotation rot)
    {
        ushort[] remappedVertices = new ushort[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            if (rot.VertexRemap.ContainsKey(i))
            {
                remappedVertices[i] = originalVertices[rot.VertexRemap[i]];
            }
            else
            {
                remappedVertices[i] = originalVertices[i];
            }
        }

        return remappedVertices;
    }
}

public class EMFaceModification
{
    public int RoomNumber { get; set; }
    public EMTextureFaceType FaceType { get; set; }
    public int FaceIndex { get; set; }
    public int[] FaceIndices { get; set; }
    public Dictionary<int, TRVertex> VertexChanges { get; set; }

    public int[] GetIndices()
    {
        return FaceIndices ?? new int[] { FaceIndex };
    }
}

public class EMFaceRotation
{
    public int RoomNumber { get; set; }
    public EMTextureFaceType FaceType { get; set; }
    public int[] FaceIndices { get; set; }
    public Dictionary<int, int> VertexRemap { get; set; }
}
