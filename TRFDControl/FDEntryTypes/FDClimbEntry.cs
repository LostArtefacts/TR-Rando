namespace TRFDControl.FDEntryTypes;

public class FDClimbEntry : FDEntry
{
    public bool IsPositiveX
    {
        get
        {
            return (Setup.SubFunction & (byte)FDClimbDirection.PositiveX) > 0;
        }
        set
        {
            if (value)
            {
                Setup.SubFunction |= (byte)FDClimbDirection.PositiveX;
            }
            else
            {
                Setup.SubFunction = (byte)(Setup.SubFunction & ~(byte)FDClimbDirection.PositiveX);
            }
        }
    }

    public bool IsPositiveZ
    {
        get
        {
            return (Setup.SubFunction & (byte)FDClimbDirection.PositiveZ) > 0;
        }
        set
        {
            if (value)
            {
                Setup.SubFunction |= (byte)FDClimbDirection.PositiveZ;
            }
            else
            {
                Setup.SubFunction = (byte)(Setup.SubFunction & ~(byte)FDClimbDirection.PositiveZ);
            }
        }
    }

    public bool IsNegativeX
    {
        get
        {
            return (Setup.SubFunction & (byte)FDClimbDirection.NegativeX) > 0;
        }
        set
        {
            if (value)
            {
                Setup.SubFunction |= (byte)FDClimbDirection.NegativeX;
            }
            else
            {
                Setup.SubFunction = (byte)(Setup.SubFunction & ~(byte)FDClimbDirection.NegativeX);
            }
        }
    }

    public bool IsNegativeZ
    {
        get
        {
            return (Setup.SubFunction & (byte)FDClimbDirection.NegativeZ) > 0;
        }
        set
        {
            if (value)
            {
                Setup.SubFunction |= (byte)FDClimbDirection.NegativeZ;
            }
            else
            {
                Setup.SubFunction = (byte)(Setup.SubFunction & ~(byte)FDClimbDirection.NegativeZ);
            }
        }
    }
}
