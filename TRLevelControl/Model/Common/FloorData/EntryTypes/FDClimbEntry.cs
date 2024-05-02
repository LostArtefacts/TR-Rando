namespace TRLevelControl.Model;

public class FDClimbEntry : FDEntry
{
    public FDClimbDirection Direction { get; set; }

    public override FDFunction GetFunction()
        => FDFunction.ClimbableWalls;

    public bool IsNegativeX
    {
        get => Direction.HasFlag(FDClimbDirection.NegativeX);
        set => SetDirection(FDClimbDirection.NegativeX, value);
    }

    public bool IsPositiveX
    {
        get => Direction.HasFlag(FDClimbDirection.PositiveX);
        set => SetDirection(FDClimbDirection.PositiveX, value);
    }

    public bool IsNegativeZ
    {
        get => Direction.HasFlag(FDClimbDirection.NegativeZ);
        set => SetDirection(FDClimbDirection.NegativeZ, value);
    }

    public bool IsPositiveZ
    {
        get => Direction.HasFlag(FDClimbDirection.PositiveZ);
        set => SetDirection(FDClimbDirection.PositiveZ, value);
    }

    public void SetDirection(FDClimbDirection direction, bool state)
    {
        if (state)
        {
            Direction |= direction;
        }
        else
        {
            Direction &= ~direction;
        }
    }

    public override FDEntry Clone()
        => (FDClimbEntry)MemberwiseClone();
}
