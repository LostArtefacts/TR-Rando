using TRLevelControl;

namespace TRDataControl.Environment;

public class EMLocationExpander
{
    public EMLocation Location { get; set; }
    public uint ExpandX { get; set; }
    public uint ExpandZ { get; set; }

    public List<EMLocation> Expand()
    {
        List<EMLocation> locs = new() { Location };
        for (int i = 0; i < ExpandX; i++)
        {
            for (int j = 0; j < ExpandZ; j++)
            {
                locs.Add(new()
                {
                    X = Location.X + i * TRConsts.Step4,
                    Y = Location.Y,
                    Z = Location.Z + j * TRConsts.Step4,
                    Room = Location.Room,
                    Angle = Location.Angle
                });
            }
        }
        return locs;
    }
}
