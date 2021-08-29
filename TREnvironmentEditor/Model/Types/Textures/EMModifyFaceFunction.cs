using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMModifyFaceFunction : BaseEMFunction
    {
        public EMFaceModification[] Modifications { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            foreach (EMFaceModification mod in Modifications)
            {
                TR2Room room = level.Rooms[mod.RoomNumber];
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

        private void ModifyRectangles(TR2Room room, EMFaceModification mod)
        {
            TRFace4 rect = room.RoomData.Rectangles[mod.FaceIndex];
            List<TR2RoomVertex> allVertices = room.RoomData.Vertices.ToList();
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR2RoomVertex currentRoomVertex = allVertices[rect.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR2RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                rect.Vertices[vertIndex] = (ushort)allVertices.Count;
                allVertices.Add(newRoomVertex);
            }

            room.RoomData.Vertices = allVertices.ToArray();
            room.RoomData.NumVertices = (short)allVertices.Count;
            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
        }

        private void ModifyTriangles(TR2Room room, EMFaceModification mod)
        {
            TRFace3 tri = room.RoomData.Triangles[mod.FaceIndex];
            List<TR2RoomVertex> allVertices = room.RoomData.Vertices.ToList();
            foreach (int vertIndex in mod.VertexChanges.Keys)
            {
                TR2RoomVertex currentRoomVertex = allVertices[tri.Vertices[vertIndex]];
                TRVertex newVertex = mod.VertexChanges[vertIndex];
                TR2RoomVertex newRoomVertex = GenerateRoomVertex(currentRoomVertex, newVertex);

                // Remap the face to use this vertex
                tri.Vertices[vertIndex] = (ushort)allVertices.Count;
                allVertices.Add(newRoomVertex);
            }

            room.RoomData.Vertices = allVertices.ToArray();
            room.RoomData.NumVertices = (short)allVertices.Count;
            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
        }

        private TR2RoomVertex GenerateRoomVertex(TR2RoomVertex currentRoomVertex, TRVertex newVertex)
        {
            // We create a new vertex because we can't guarantee nothing else is using it.
            // Note the vertex values in the mod are added to the existing values, so we
            // don't have to define those we aren't changing.
            return new TR2RoomVertex
            {
                Attributes = currentRoomVertex.Attributes,
                Lighting = currentRoomVertex.Lighting,
                Lighting2 = currentRoomVertex.Lighting2,
                Vertex = new TRVertex
                {
                    X = (short)(currentRoomVertex.Vertex.X + newVertex.X),
                    Y = (short)(currentRoomVertex.Vertex.Y + newVertex.Y),
                    Z = (short)(currentRoomVertex.Vertex.Z + newVertex.Z)
                }
            };
        }
    }

    public class EMFaceModification
    {
        public int RoomNumber { get; set; }
        public EMTextureFaceType FaceType { get; set; }
        public int FaceIndex { get; set; }
        public Dictionary<int, TRVertex> VertexChanges { get; set; }
    }
}