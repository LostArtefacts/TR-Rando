﻿using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMRemoveFaceFunction : BaseEMFunction
{
    public EMGeometryMap GeometryMap { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);

        foreach (int roomNumber in GeometryMap.Keys)
        {
            TRRoom room = level.Rooms[data.ConvertRoom(roomNumber)];
            List<int> rectangleRemovals = new();
            List<int> triangleRemovals = new();

            foreach (EMTextureFaceType faceType in GeometryMap[roomNumber].Keys)
            {
                foreach (int faceIndex in GeometryMap[roomNumber][faceType])
                {
                    switch (faceType)
                    {
                        case EMTextureFaceType.Rectangles:
                            rectangleRemovals.Add(faceIndex);
                            break;
                        case EMTextureFaceType.Triangles:
                            triangleRemovals.Add(faceIndex);
                            break;
                    }
                }
            }

            room.RoomData.Rectangles = RemoveEntries(room.RoomData.Rectangles, rectangleRemovals);
            room.RoomData.Triangles = RemoveEntries(room.RoomData.Triangles, triangleRemovals);

            room.RoomData.NumRectangles = (short)room.RoomData.Rectangles.Length;
            room.RoomData.NumTriangles = (short)room.RoomData.Triangles.Length;

            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);

        foreach (int roomNumber in GeometryMap.Keys)
        {
            TR2Room room = level.Rooms[data.ConvertRoom(roomNumber)];
            List<int> rectangleRemovals = new();
            List<int> triangleRemovals = new();

            foreach (EMTextureFaceType faceType in GeometryMap[roomNumber].Keys)
            {
                foreach (int faceIndex in GeometryMap[roomNumber][faceType])
                {
                    switch (faceType)
                    {
                        case EMTextureFaceType.Rectangles:
                            rectangleRemovals.Add(faceIndex);
                            break;
                        case EMTextureFaceType.Triangles:
                            triangleRemovals.Add(faceIndex);
                            break;
                    }
                }
            }

            room.RoomData.Rectangles = RemoveEntries(room.RoomData.Rectangles, rectangleRemovals);
            room.RoomData.Triangles = RemoveEntries(room.RoomData.Triangles, triangleRemovals);

            room.RoomData.NumRectangles = (short)room.RoomData.Rectangles.Length;
            room.RoomData.NumTriangles = (short)room.RoomData.Triangles.Length;

            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);

        foreach (int roomNumber in GeometryMap.Keys)
        {
            TR3Room room = level.Rooms[data.ConvertRoom(roomNumber)];
            List<int> rectangleRemovals = new();
            List<int> triangleRemovals = new();

            foreach (EMTextureFaceType faceType in GeometryMap[roomNumber].Keys)
            {
                foreach (int faceIndex in GeometryMap[roomNumber][faceType])
                {
                    switch (faceType)
                    {
                        case EMTextureFaceType.Rectangles:
                            rectangleRemovals.Add(faceIndex);
                            break;
                        case EMTextureFaceType.Triangles:
                            triangleRemovals.Add(faceIndex);
                            break;
                    }
                }
            }

            room.RoomData.Rectangles = RemoveEntries(room.RoomData.Rectangles, rectangleRemovals);
            room.RoomData.Triangles = RemoveEntries(room.RoomData.Triangles, triangleRemovals);

            room.RoomData.NumRectangles = (short)room.RoomData.Rectangles.Length;
            room.RoomData.NumTriangles = (short)room.RoomData.Triangles.Length;

            room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
        }
    }

    private static T[] RemoveEntries<T>(T[] items, List<int> indices)
    {
        List<T> itemList = items.ToList();
        
        indices.Sort();
        for (int i = indices.Count - 1; i >= 0; i--)
        {
            itemList.RemoveAt(indices[i]);
        }

        return itemList.ToArray();
    }
}
