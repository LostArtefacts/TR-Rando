using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAddFaceFunction : BaseEMFunction
    {
        public Dictionary<short, List<TRFace4>> Quads { get; set; }
        public Dictionary<short, List<TRFace3>> Triangles { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            if (Quads != null)
            {
                foreach (short roomIndex in Quads.Keys)
                {
                    TR2Room room = level.Rooms[ConvertItemNumber(roomIndex, level.NumRooms)];
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
                    TR2Room room = level.Rooms[ConvertItemNumber(roomIndex, level.NumRooms)];
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
            if (Quads != null)
            {
                foreach (short roomIndex in Quads.Keys)
                {
                    TR3Room room = level.Rooms[ConvertItemNumber(roomIndex, level.NumRooms)];
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
                    TR3Room room = level.Rooms[ConvertItemNumber(roomIndex, level.NumRooms)];
                    List<TRFace3> faces = room.RoomData.Triangles.ToList();
                    faces.AddRange(Triangles[roomIndex]);

                    room.RoomData.Triangles = faces.ToArray();
                    room.RoomData.NumTriangles = (short)faces.Count;

                    room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
                }
            }
        }
    }
}