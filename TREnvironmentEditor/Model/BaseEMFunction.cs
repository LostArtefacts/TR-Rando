using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public abstract class BaseEMFunction
    {
        // Y click size
        public static readonly int ClickSize = 256;
        // Sector X/Z length
        public static readonly int SectorSize = 1024;

        [JsonProperty(Order = -2)]
        public string Comments { get; set; }
        [JsonProperty(Order = -2, DefaultValueHandling = DefaultValueHandling.Include)]
        public EMType EMType { get; set; }

        public abstract void ApplyToLevel(TRLevel level);
        public abstract void ApplyToLevel(TR2Level level);
        public abstract void ApplyToLevel(TR3Level level);

        /// <summary>
        /// Gets the expected vertices for a flat tile.
        /// asCeiling = true  => looking up
        /// asCeiling = false => looking down
        /// </summary>
        public List<TRVertex> GetTileVertices(short x, short y, short z, bool asCeiling)
        {
            List<TRVertex> vertices = new List<TRVertex>
            {
                new TRVertex { X = (short)(x + SectorSize), Y = y, Z = z },
                new TRVertex { X = x, Y = y, Z = z },
                new TRVertex { X = x, Y = y, Z = (short)(z + SectorSize) },
                new TRVertex { X = (short)(x + SectorSize), Y = y, Z = (short)(z + SectorSize) }
            };

            if (asCeiling)
            {
                vertices.Reverse();
            }

            return vertices;
        }

        public int CreateRoomVertex(TRRoom room, TRVertex vert, short lighting = 6574)
        {
            TRRoomVertex v = new TRRoomVertex
            {
                Lighting = lighting,
                Vertex = vert
            };

            List<TRRoomVertex> verts = room.RoomData.Vertices.ToList();
            verts.Add(v);
            room.RoomData.Vertices = verts.ToArray();
            room.RoomData.NumVertices++;
            return verts.Count - 1;
        }

        public int CreateRoomVertex(TR2Room room, TRVertex vert, short lighting = 6574, short lighting2 = 6574)
        {
            TR2RoomVertex v = new TR2RoomVertex
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

        public int CreateRoomVertex(TR3Room room, TRVertex vert, short lighting = 6574, ushort colour = 6574, bool useCaustics = false, bool useWaveMovement = false)
        {
            TR3RoomVertex v = new TR3RoomVertex
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
        public ISet<byte> GetAdjacentRooms(IEnumerable<TRRoomSector> sectors, bool above)
        {
            ISet<byte> rooms = new HashSet<byte>();
            foreach (TRRoomSector sector in sectors)
            {
                byte roomNumber = above ? sector.RoomAbove : sector.RoomBelow;
                if (roomNumber != 255)
                {
                    rooms.Add(roomNumber);
                }
            }
            return rooms;
        }

        protected EMLevelData GetData(TRLevel level)
        {
            return new EMLevelData
            {
                NumCameras = level.NumCameras,
                NumEntities = level.NumEntities,
                NumRooms = level.NumRooms
            };
        }

        protected EMLevelData GetData(TR2Level level)
        {
            return new EMLevelData
            {
                NumCameras = level.NumCameras,
                NumEntities = level.NumEntities,
                NumRooms = level.NumRooms
            };
        }

        protected EMLevelData GetData(TR3Level level)
        {
            return new EMLevelData
            {
                NumCameras = level.NumCameras,
                NumEntities = level.NumEntities,
                NumRooms = level.NumRooms
            };
        }
    }
}