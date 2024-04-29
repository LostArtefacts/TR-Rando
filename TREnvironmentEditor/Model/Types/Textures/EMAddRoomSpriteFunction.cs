using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMAddRoomSpriteFunction : BaseEMFunction
{
    public short Texture { get; set; }
    public EMRoomVertex Vertex { get; set; }
    public List<EMLocation> Locations { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        foreach (EMLocation location in Locations)
        {
            TR1Room room = level.Rooms[data.ConvertRoom(location.Room)];
            List<TR1RoomVertex> vertices = room.RoomData.Vertices.ToList();
            List<TRRoomSprite> sprites = room.RoomData.Sprites.ToList();
            sprites.Add(new()
            {
                Texture = Texture,
                Vertex = (short)vertices.Count
            });
            vertices.Add(new()
            {
                Lighting = Vertex.Lighting,
                Vertex = new()
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
            sprites.Add(new()
            {
                Texture = Texture,
                Vertex = (short)vertices.Count
            });
            vertices.Add(new()
            {
                Lighting = Vertex.Lighting,
                Lighting2 = Vertex.Lighting2,
                Attributes = Vertex.Attributes,
                Vertex = new()
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
            sprites.Add(new()
            {
                Texture = Texture,
                Vertex = (short)vertices.Count
            });
            vertices.Add(new()
            {
                Lighting = Vertex.Lighting,
                Attributes = Vertex.Attributes,
                Colour = Vertex.Colour,
                Vertex = new()
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
