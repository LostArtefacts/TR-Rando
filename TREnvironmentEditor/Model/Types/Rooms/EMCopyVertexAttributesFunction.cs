using System;
using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMCopyVertexAttributesFunction : BaseEMFunction
    {
        public Dictionary<short, Dictionary<EMTextureFaceType, Dictionary<int, int>>> FaceMap { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            throw new NotImplementedException();
        }

        public override void ApplyToLevel(TR3Level level)
        {
            foreach (short roomNumber in FaceMap.Keys)
            {
                TR3Room room = level.Rooms[ConvertItemNumber(roomNumber, level.NumRooms)];
                foreach (EMTextureFaceType faceType in FaceMap[roomNumber].Keys)
                {
                    foreach (int baseFaceIndex in FaceMap[roomNumber][faceType].Keys)
                    {
                        ushort[] baseVertices, copyVertices;
                        switch (faceType)
                        {
                            case EMTextureFaceType.Rectangles:
                                baseVertices = room.RoomData.Rectangles[baseFaceIndex].Vertices;
                                copyVertices = room.RoomData.Rectangles[FaceMap[roomNumber][faceType][baseFaceIndex]].Vertices;
                                break;
                            case EMTextureFaceType.Triangles:
                                baseVertices = room.RoomData.Triangles[baseFaceIndex].Vertices;
                                copyVertices = room.RoomData.Triangles[FaceMap[roomNumber][faceType][baseFaceIndex]].Vertices;
                                break;
                            default:
                                throw new ArgumentException();
                        }
                        
                        for (int i = 0; i < baseVertices.Length; i++)
                        {
                            TR3RoomVertex baseVertex = room.RoomData.Vertices[baseVertices[i]];
                            TR3RoomVertex copyVertex = room.RoomData.Vertices[copyVertices[i]];
                            copyVertex.Attributes = baseVertex.Attributes;
                            copyVertex.Colour = baseVertex.Colour;
                            copyVertex.Lighting = baseVertex.Lighting;
                        }
                    }
                }
            }
        }
    }
}