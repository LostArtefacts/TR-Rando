using System.Collections.Generic;

namespace TREnvironmentEditor.Helpers
{
    public class EMLocationExpander
    {
        public EMLocation Location { get; set; }
        public uint ExpandX { get; set; }
        public uint ExpandZ { get; set; }

        public List<EMLocation> Expand()
        {
            List<EMLocation> locs = new List<EMLocation> { Location };
            for (int i = 0; i < ExpandX; i++)
            {
                for (int j = 0; j < ExpandZ; j++)
                {
                    locs.Add(new EMLocation
                    {
                        X = Location.X + i * 1024,
                        Y = Location.Y,
                        Z = Location.Z + j * 1024,
                        Room = Location.Room,
                        Angle = Location.Angle
                    });
                }
            }
            return locs;
        }
    }
}