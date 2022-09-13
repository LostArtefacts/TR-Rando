
using TRViewInterop.Routes;

namespace TRRandomizerCore.Helpers
{
    public class Location
    {
        public int X { get; set; }

        public int Z { get; set; }

        public int Y { get; set; }

        public int Room { get; set; }
        public short Angle { get; set; }

        public bool RequiresGlitch { get; set; }

        public Difficulty Difficulty { get; set; }

        public bool IsInRoomSpace { get; set; }

        public bool IsItem { get; set; }

        public bool VehicleRequired { get; set; }

        public bool Validated { get; set; }

        public bool RequiresDamage { get; set; }

        public int KeyItemGroupID { get; set; }

        public bool InvalidatesRoom { get; set; }

        public LevelState LevelState { get; set; }

        public int EntityIndex { get; set; }
        public short TargetType { get; set; }

        public Location()
        {
            X = 0;
            Y = 0;
            Z = 0;
            Room = 0;
            Angle = 16384;
            RequiresGlitch = false;
            Difficulty = Difficulty.Easy;
            IsInRoomSpace = false;
            IsItem = false;
            VehicleRequired = false;
            Validated = true;
            RequiresDamage = false;
            KeyItemGroupID = 0;
            InvalidatesRoom = false;
            LevelState = LevelState.Any;
            EntityIndex = -1;
            TargetType = -1;
        }

        public Location(TRViewLocation loc)
        {
            X = loc.X;
            Y = loc.Y;
            Z = loc.Z;
            Room = loc.Room;
            Angle = 16384;
        }

        /// <summary>
        /// Compare the location to the location in parameter only in a matter of X;Y;Z and Room
        /// </summary>
        /// <param name="locationToTest"></param>
        /// <returns>True if they are the same</returns>
        public bool IsTheSame(Location locationToTest)
        {
            if (X == locationToTest.X &&
                Y == locationToTest.Y &&
                Z == locationToTest.Z &&
                Room == locationToTest.Room) return true;
            else return false;
        }


    }
}
