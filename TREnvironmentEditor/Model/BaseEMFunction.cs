using System.Collections.Generic;
using System.Linq;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model
{
    public abstract class BaseEMFunction
    {
        // Y click size
        public static readonly int ClickSize = 256;
        // Sector X/Z length
        public static readonly int SectorSize = 1024;

        public EMType EMType { get; set; }

        public abstract void ApplyToLevel(TR2Level level);

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

        public int CreateRoomVertex(TR2Room room, TRVertex vert)
        {
            TR2RoomVertex v = new TR2RoomVertex
            {
                Attributes = 32784, // This stops it shimmering if viewed from underwater, should be configuratble
                Lighting = 6574, // Needs to be configurable
                Lighting2 = 6574,// Needs to be configurable
                Vertex = vert
            };

            List<TR2RoomVertex> verts = room.RoomData.Vertices.ToList();
            verts.Add(v);
            room.RoomData.Vertices = verts.ToArray();
            room.RoomData.NumVertices++;
            return verts.Count - 1;
        }

        /// <summary>
        /// Gets the indices of rooms above or below the provided room.
        /// </summary>
        public ISet<byte> GetAdjacentRooms(TR2Room room, bool above)
        {
            ISet<byte> rooms = new HashSet<byte>();
            foreach (TRRoomSector sector in room.SectorList)
            {
                byte roomNumber = above ? sector.RoomAbove : sector.RoomBelow;
                if (roomNumber != 255)
                {
                    rooms.Add(roomNumber);
                }
            }
            return rooms;
        }
    }
}