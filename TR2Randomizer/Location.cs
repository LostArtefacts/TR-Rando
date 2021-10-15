using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRViewInterop.Routes;

namespace TR2Randomizer
{
    public class Location
    {
        public int X { get; set; }

        public int Z { get; set; }

        public int Y { get; set; }

        public int Room { get; set; }

        public bool RequiresGlitch { get; set; }

        public Difficulty Difficulty { get; set; }

        public bool IsInRoomSpace { get; set; }

        public bool IsItem { get; set; }

        public bool VehicleRequired { get; set; }

        public Location()
        {
            X = 0;
            Y = 0;
            Z = 0;
            Room = 0;
            RequiresGlitch = false;
            Difficulty = Difficulty.Easy;
            IsInRoomSpace = false;
            IsItem = false;
            VehicleRequired = false;
        }

        public Location(TRViewLocation loc)
        {
            X = loc.X;
            Y = loc.Y;
            Z = loc.Z;
            Room = loc.Room;
        }
    }
}
