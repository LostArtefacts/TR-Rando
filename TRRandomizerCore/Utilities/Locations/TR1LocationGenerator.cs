using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using TRFDControl.Utilities;
using TRLevelReader.Model;
using TRRandomizerCore.Helpers;

namespace TRRandomizerCore.Utilities
{
    public class TR1LocationGenerator : AbstractLocationGenerator<TRLevel>
    {
        public override bool CrawlspacesAllowed => false;

        protected override void ReadFloorData(TRLevel level)
        {
            _floorData.ParseFromLevel(level);
        }

        protected override TRRoomSector GetSector(Location location, TRLevel level)
        {
            return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, (short)location.Room, level, _floorData);
        }

        protected override List<TRRoomSector> GetRoomSectors(TRLevel level, int room)
        {
            return level.Rooms[room].Sectors.ToList();
        }

        protected override List<TRStaticMesh> GetStaticMeshes(TRLevel level)
        {
            return level.StaticMeshes.ToList();
        }

        protected override int GetRoomCount(TRLevel level)
        {
            return level.NumRooms;
        }

        protected override short GetFlipMapRoom(TRLevel level, short room)
        {
            return level.Rooms[room].AlternateRoom;
        }

        protected override bool IsRoomValid(TRLevel level, short room)
        {
            return true;
        }

        protected override Dictionary<ushort, List<Location>> GetRoomStaticMeshLocations(TRLevel level, short room)
        {
            Dictionary<ushort, List<Location>> locations = new Dictionary<ushort, List<Location>>();
            foreach (TRRoomStaticMesh staticMesh in level.Rooms[room].StaticMeshes)
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

        protected override ushort GetRoomDepth(TRLevel level, short room)
        {
            return level.Rooms[room].NumZSectors;
        }

        protected override int GetRoomYTop(TRLevel level, short room)
        {
            return level.Rooms[room].Info.YTop;
        }

        protected override Vector2 GetRoomPosition(TRLevel level, short room)
        {
            return new Vector2(level.Rooms[room].Info.X, level.Rooms[room].Info.Z);
        }
    }
}