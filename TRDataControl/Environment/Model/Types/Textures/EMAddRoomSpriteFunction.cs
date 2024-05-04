using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMAddRoomSpriteFunction : BaseEMFunction
{
    public uint ID { get; set; }
    public EMRoomVertex Vertex { get; set; }
    public List<EMLocation> Locations { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        foreach (EMLocation location in Locations)
        {
            TR1Room room = level.Rooms[data.ConvertRoom(location.Room)];
            room.Mesh.Sprites.Add(new()
            {
                ID = (TR1Type)ID,
                Vertex = (short)room.Mesh.Vertices.Count
            });
            room.Mesh.Vertices.Add(new()
            {
                Lighting = Vertex.Lighting,
                Vertex = new()
                {
                    X = (short)(location.X - room.Info.X),
                    Y = (short)location.Y,
                    Z = (short)(location.Z - room.Info.Z)
                }
            });
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        foreach (EMLocation location in Locations)
        {
            TR2Room room = level.Rooms[data.ConvertRoom(location.Room)];
            room.Mesh.Sprites.Add(new()
            {
                ID = (TR2Type)ID,
                Vertex = (short)room.Mesh.Vertices.Count
            });
            room.Mesh.Vertices.Add(new()
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
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        foreach (EMLocation location in Locations)
        {
            TR3Room room = level.Rooms[data.ConvertRoom(location.Room)];
            room.Mesh.Sprites.Add(new()
            {
                ID = (TR3Type)ID,
                Vertex = (short)room.Mesh.Vertices.Count
            });
            room.Mesh.Vertices.Add(new()
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
        }
    }
}
