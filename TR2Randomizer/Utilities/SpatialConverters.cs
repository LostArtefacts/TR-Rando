using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Model;

namespace TR2Randomizer.Utilities
{
    internal static class SpatialConverters
    {
        internal static Location TransformToLevelSpace(Location loc, TR2Room room)
        {
            if (loc.IsInRoomSpace)
            {
                loc.X = (loc.X + room.Info.X);
                loc.Y = (room.Info.YBottom - loc.Y);
                loc.Z = (loc.Z + room.Info.Z);
            }

            return loc;
        }
    }
}
