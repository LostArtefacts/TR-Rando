using Newtonsoft.Json;
using TREnvironmentEditor.Helpers;
using TRLevelControl;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model;

public abstract class BaseEMFunction
{
    [JsonProperty(Order = -2)]
    public string Comments { get; set; }
    [JsonProperty(Order = -2, DefaultValueHandling = DefaultValueHandling.Include)]
    public EMType EMType { get; set; }

    public BaseEMFunction HardVariant { get; set; }
    public List<EMTag> Tags { get; set; }

    protected bool _isCommunityPatch;

    public abstract void ApplyToLevel(TR1Level level);
    public abstract void ApplyToLevel(TR2Level level);
    public abstract void ApplyToLevel(TR3Level level);

    public void SetCommunityPatch(bool isCommunityPatch)
        => _isCommunityPatch = isCommunityPatch;

    /// <summary>
    /// Gets the expected vertices for a flat tile.
    /// asCeiling = true  => looking up
    /// asCeiling = false => looking down
    /// </summary>
    public static List<TRVertex> GetTileVertices(short x, short y, short z, bool asCeiling)
    {
        List<TRVertex> vertices = new()
        {
            new TRVertex { X = (short)(x + TRConsts.Step4), Y = y, Z = z },
            new TRVertex { X = x, Y = y, Z = z },
            new TRVertex { X = x, Y = y, Z = (short)(z + TRConsts.Step4) },
            new TRVertex { X = (short)(x + TRConsts.Step4), Y = y, Z = (short)(z + TRConsts.Step4) }
        };

        if (asCeiling)
        {
            vertices.Reverse();
        }

        return vertices;
    }

    public static int CreateRoomVertex(TR1Room room, TRVertex vert, short lighting = 6574)
    {
        TR1RoomVertex v = new()
        {
            Lighting = lighting,
            Vertex = vert
        };

        List<TR1RoomVertex> verts = room.RoomData.Vertices.ToList();
        verts.Add(v);
        room.RoomData.Vertices = verts.ToArray();
        room.RoomData.NumVertices++;
        return verts.Count - 1;
    }

    public static int CreateRoomVertex(TR2Room room, TRVertex vert, short lighting = 6574, short lighting2 = 6574)
    {
        TR2RoomVertex v = new()
        {
            Attributes = 32784, // This stops it shimmering if viewed from underwater, should be configurable
            Lighting = lighting,
            Lighting2 = lighting2,
            Vertex = vert
        };

        List<TR2RoomVertex> verts = room.RoomData.Vertices.ToList();
        verts.Add(v);
        room.RoomData.Vertices = verts.ToArray();
        room.RoomData.NumVertices++;
        return verts.Count - 1;
    }

    public static int CreateRoomVertex(TR3Room room, TRVertex vert, short lighting = 6574, ushort colour = 6574, bool useCaustics = false, bool useWaveMovement = false)
    {
        TR3RoomVertex v = new()
        {
            Attributes = 32784,
            Lighting = lighting,
            Colour = colour,
            UseCaustics = useCaustics,
            UseWaveMovement = useWaveMovement,
            Vertex = vert
        };

        List<TR3RoomVertex> verts = room.RoomData.Vertices.ToList();
        verts.Add(v);
        room.RoomData.Vertices = verts.ToArray();
        room.RoomData.NumVertices++;
        return verts.Count - 1;
    }

    /// <summary>
    /// Gets the indices of rooms above or below the provided room.
    /// </summary>
    public static ISet<byte> GetAdjacentRooms(IEnumerable<TRRoomSector> sectors, bool above)
    {
        ISet<byte> rooms = new HashSet<byte>();
        foreach (TRRoomSector sector in sectors)
        {
            byte roomNumber = above ? sector.RoomAbove : sector.RoomBelow;
            if (roomNumber != TRConsts.NoRoom)
            {
                rooms.Add(roomNumber);
            }
        }
        return rooms;
    }

    protected static EMLevelData GetData(TR1Level level)
    {
        return EMLevelData.GetData(level);
    }

    protected static EMLevelData GetData(TR2Level level)
    {
        return EMLevelData.GetData(level);
    }

    protected static EMLevelData GetData(TR3Level level)
    {
        return EMLevelData.GetData(level);
    }
}
