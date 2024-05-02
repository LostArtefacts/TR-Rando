namespace TRLevelControl.Model;

public interface ITRLocatable
{
    int X { get; set; }
    int Y { get; set; }
    int Z { get; set; }
    short Room { get; set; }
    short Angle { get; set; }
}
