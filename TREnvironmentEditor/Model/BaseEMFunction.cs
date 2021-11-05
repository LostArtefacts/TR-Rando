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

        public int CreateRoomVertex(TR2Room room, TRVertex vert, short lighting = 6574, short lighting2 = 6574)
        {
            TR2RoomVertex v = new TR2RoomVertex
            {
                Attributes = 32784, // This stops it shimmering if viewed from underwater, should be configuratble
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

        // This allows us to access the last item in a specific list, so if for example one mod has created
        // a new room, subsequent mods can retrieve its number using short.MaxValue.
        protected int ConvertItemNumber(int itemNumber, ushort numItems)
        {
            if (itemNumber == short.MaxValue)
            {
                itemNumber = numItems - 1;
            }
            return itemNumber;
        }
    }
}