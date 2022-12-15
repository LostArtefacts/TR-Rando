using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMAddRoomSpriteFunction : BaseEMFunction
    {
        public short Texture { get; set; }
        public EMRoomVertex Vertex { get; set; }
        public List<EMLocation> Locations { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);
            foreach (EMLocation location in Locations)
            {
                TRRoom room = level.Rooms[data.ConvertRoom(location.Room)];
                List<TRRoomVertex> vertices = room.RoomData.Vertices.ToList();
                List<TRRoomSprite> sprites = room.RoomData.Sprites.ToList();
                sprites.Add(new TRRoomSprite
                {
                    Texture = Texture,
                    Vertex = (short)vertices.Count
                });
                vertices.Add(new TRRoomVertex
                {
                    Lighting = Vertex.Lighting,
                    Vertex = new TRVertex
                    {
                        X = (short)(location.X - room.Info.X),
                        Y = (short)location.Y,
                        Z = (short)(location.Z - room.Info.Z)
                    }
                });

                room.RoomData.Vertices = vertices.ToArray();
                room.RoomData.Sprites = sprites.ToArray();
                room.RoomData.NumSprites++;
                room.RoomData.NumVertices++;

                room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);
            foreach (EMLocation location in Locations)
            {
                TR2Room room = level.Rooms[data.ConvertRoom(location.Room)];
                List<TR2RoomVertex> vertices = room.RoomData.Vertices.ToList();
                List<TRRoomSprite> sprites = room.RoomData.Sprites.ToList();
                sprites.Add(new TRRoomSprite
                {
                    Texture = Texture,
                    Vertex = (short)vertices.Count
                });
                vertices.Add(new TR2RoomVertex
                {
                    Lighting = Vertex.Lighting,
                    Lighting2 = Vertex.Lighting2,
                    Attributes = Vertex.Attributes,
                    Vertex = new TRVertex
                    {
                        X = (short)(location.X - room.Info.X),
                        Y = (short)location.Y,
                        Z = (short)(location.Z - room.Info.Z)
                    }
                });

                room.RoomData.Vertices = vertices.ToArray();
                room.RoomData.Sprites = sprites.ToArray();
                room.RoomData.NumSprites++;
                room.RoomData.NumVertices++;

                room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);
            foreach (EMLocation location in Locations)
            {
                TR3Room room = level.Rooms[data.ConvertRoom(location.Room)];
                List<TR3RoomVertex> vertices = room.RoomData.Vertices.ToList();
                List<TRRoomSprite> sprites = room.RoomData.Sprites.ToList();
                sprites.Add(new TRRoomSprite
                {
                    Texture = Texture,
                    Vertex = (short)vertices.Count
                });
                vertices.Add(new TR3RoomVertex
                {
                    Lighting = Vertex.Lighting,
                    Attributes = Vertex.Attributes,
                    Colour = Vertex.Colour,
                    Vertex = new TRVertex
                    {
                        X = (short)(location.X - room.Info.X),
                        Y = (short)location.Y,
                        Z = (short)(location.Z - room.Info.Z)
                    }
                });

                room.RoomData.Vertices = vertices.ToArray();
                room.RoomData.Sprites = sprites.ToArray();
                room.RoomData.NumSprites++;
                room.RoomData.NumVertices++;

                room.NumDataWords = (uint)(room.RoomData.Serialize().Length / 2);
            }
        }
    }
}
