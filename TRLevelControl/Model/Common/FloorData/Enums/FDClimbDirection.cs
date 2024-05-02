namespace TRLevelControl.Model;

[Flags]
public enum FDClimbDirection
{
    PositiveZ = 1 << 0,
    PositiveX = 1 << 1,
    NegativeZ = 1 << 2,
    NegativeX = 1 << 3,
}
