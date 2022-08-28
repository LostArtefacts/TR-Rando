using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAddFaceFunction : BaseEMFunction, ITextureModifier
    {
        public Dictionary<short, List<TRFace4>> Quads { get; set; }
        public Dictionary<short, List<TRFace3>> Triangles { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);

            if (Quads != null)
            {
                foreach (short roomIndex in Quads.Keys)
                {
                    TRRoom room = level.Rooms[data.ConvertRoom(roomIndex)];
                    List<TRFace4> faces = room.RoomData.Rectangles.ToList();
                    faces.AddRange(Quads[roomIndex]);

                    room.RoomData.Rectangles = faces.ToArray();
                    room.RoomData.NumRectangles = (short)faces.Count;

                    room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
                }
            }

            if (Triangles != null)
            {
                foreach (short roomIndex in Triangles.Keys)
                {
                    TRRoom room = level.Rooms[data.ConvertRoom(roomIndex)];
                    List<TRFace3> faces = room.RoomData.Triangles.ToList();
                    faces.AddRange(Triangles[roomIndex]);

                    room.RoomData.Triangles = faces.ToArray();
                    room.RoomData.NumTriangles = (short)faces.Count;

                    room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
                }
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            if (Quads != null)
            {
                foreach (short roomIndex in Quads.Keys)
                {
                    TR2Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                    List<TRFace4> faces = room.RoomData.Rectangles.ToList();
                    faces.AddRange(Quads[roomIndex]);

                    room.RoomData.Rectangles = faces.ToArray();
                    room.RoomData.NumRectangles = (short)faces.Count;

                    room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
                }
            }

            if (Triangles != null)
            {
                foreach (short roomIndex in Triangles.Keys)
                {
                    TR2Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                    List<TRFace3> faces = room.RoomData.Triangles.ToList();
                    faces.AddRange(Triangles[roomIndex]);

                    room.RoomData.Triangles = faces.ToArray();
                    room.RoomData.NumTriangles = (short)faces.Count;

                    room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
                }
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);

            if (Quads != null)
            {
                foreach (short roomIndex in Quads.Keys)
                {
                    TR3Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                    List<TRFace4> faces = room.RoomData.Rectangles.ToList();
                    faces.AddRange(Quads[roomIndex]);

                    room.RoomData.Rectangles = faces.ToArray();
                    room.RoomData.NumRectangles = (short)faces.Count;

                    room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
                }
            }

            if (Triangles != null)
            {
                foreach (short roomIndex in Triangles.Keys)
                {
                    TR3Room room = level.Rooms[data.ConvertRoom(roomIndex)];
                    List<TRFace3> faces = room.RoomData.Triangles.ToList();
                    faces.AddRange(Triangles[roomIndex]);

                    room.RoomData.Triangles = faces.ToArray();
                    room.RoomData.NumTriangles = (short)faces.Count;

                    room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
                }
            }
        }

        public void RemapTextures(Dictionary<ushort, ushort> indexMap)
        {
            if (Quads != null)
            {
                foreach (List<TRFace4> faces in Quads.Values)
                {
                    foreach (TRFace4 face in faces)
                    {
                        if (indexMap.ContainsKey(face.Texture))
                        {
                            face.Texture = indexMap[face.Texture];
                        }
                    }
                }
            }

            if (Triangles != null)
            {
                foreach (List<TRFace3> faces in Triangles.Values)
                {
                    foreach (TRFace3 face in faces)
                    {
                        if (indexMap.ContainsKey(face.Texture))
                        {
                            face.Texture = indexMap[face.Texture];
                        }
                    }
                }
            }
        }
    }
}