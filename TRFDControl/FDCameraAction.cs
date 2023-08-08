namespace TRFDControl;

public class FDCameraAction
{
    public ushort Value { get; set; }

    public byte Timer
    {
        get
        {
            return (byte)(Value & 0x00FF);
        }
        set
        {
            Value = (ushort)(Value & ~(Value & 0x00FF));
            Value |= value;
        }
    }

    public bool Once
    {
        get
        {
            return (Value & 0x0100) > 0;
        }
        set
        {
            if (value)
            {
                Value |= 0x0100;
            }
            else
            {
                Value = (ushort)(Value & ~0x0100);
            }
        }
    }

    public byte MoveTimer
    {
        get
        {
            return (byte)((Value & 0x3E00) >> 9);
        }
        set
        {
            Value = (ushort)(Value & ~(Value & 0x3E00));
            Value |= (ushort)(value << 9);
        }
    }

    public bool Continue
    {
        get
        {
            //Continue bit set to 0 means to continue, not 1...
            return !((Value & 0x8000) > 0);
        }
        internal set
        {
            if (value)
            {
                Value = (ushort)(Value & ~0x8000);
            }
            else
            {
                Value |= 0x8000;
            }
        }
    }
}
