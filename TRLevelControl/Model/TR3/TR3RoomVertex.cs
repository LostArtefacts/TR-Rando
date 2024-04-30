namespace TRLevelControl.Model;

public class TR3RoomVertex : TRRoomVertex
{
    public short Lighting { get; set; }
    public ushort Attributes { get; set; }
    public ushort Colour { get; set; }

    public bool UseWaveMovement
    {
        get => (Attributes & 0x2000) > 0;
        set
        {
            if (value)
            {
                Attributes |= 0x2000;
            }
            else
            {
                Attributes = (ushort)(Attributes & ~0x2000);
            }
        }
    }

    public bool UseCaustics
    {
        get => (Attributes & 0x4000) > 0;
        set
        {
            if (value)
            {
                Attributes |= 0x4000;
            }
            else
            {
                Attributes = (ushort)(Attributes & ~0x4000);
            }
        }
    }
}
