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

        public int KeyItemGroupID { get; set; }

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
            KeyItemGroupID = 0;
        }

        public Location(TRViewLocation loc)
        {
            X = loc.X;
            Y = loc.Y;
            Z = loc.Z;
            Room = loc.Room;
            Angle = 16384;
        }
    }
}
