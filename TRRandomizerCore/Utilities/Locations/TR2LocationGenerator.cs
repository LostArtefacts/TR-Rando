using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TRFDControl.Utilities;
using TRLevelReader.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities
{
    public class TR2LocationGenerator : AbstractLocationGenerator<TR2Level>
    {
        public override bool CrawlspacesAllowed => false;

        protected override void ReadFloorData(TR2Level level)
        {
            _floorData.ParseFromLevel(level);
        }

        protected override TRRoomSector GetSector(Location location, TR2Level level)
        {
            return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room, level, _floorData);
        }

        protected override List<TRRoomSector> GetRoomSectors(TR2Level level, int room)
        {
            return level.Rooms[room].SectorList.ToList();
        }

        protected override List<TRStaticMesh> GetStaticMeshes(TR2Level level)
        {
            return level.StaticMeshes.ToList();
        }

        protected override int GetRoomCount(TR2Level level)
        {
            return level.NumRooms;
        }

        protected override short GetFlipMapRoom(TR2Level level, short room)
        {
            return level.Rooms[room].AlternateRoom;
        }

        protected override bool IsRoomValid(TR2Level level, short room)
        {
            return true;
        }

        protected override Dictionary<ushort, List<Location>> GetRoomStaticMeshLocations(TR2Level level, short room)
        {
            Dictionary<ushort, List<Location>> locations = new Dictionary<ushort, List<Location>>();
            foreach (TR2RoomStaticMesh staticMesh in level.Rooms[room].StaticMeshes)
            {
                if (!locations.ContainsKey(staticMesh.MeshID))
                {
                    locations[staticMesh.MeshID] = new List<Location>();
                }

                locations[staticMesh.MeshID].Add(new Location
                {
                    X = (int)staticMesh.X,
                    Y = (int)staticMesh.Y,
                    Z = (int)staticMesh.Z,
                    Room = room
                });
            }

            return locations;
        }

        protected override ushort GetRoomDepth(TR2Level level, short room)
        {
            return level.Rooms[room].NumZSectors;
        }

        protected override int GetRoomYTop(TR2Level level, short room)
        {
            return level.Rooms[room].Info.YTop;
        }

        protected override Vector2 GetRoomPosition(TR2Level level, short room)
        {
            return new Vector2(level.Rooms[room].Info.X, level.Rooms[room].Info.Z);
        }
    }
}