namespace TRLevelControl.Model;

public class TREntity<T>
    where T : Enum
{
    public T TypeID { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public short Room { get; set; }
    public short Angle { get; set; }
    public ushort Flags { get; set; }

    public bool ClearBody
    {
        get => (Flags & 0x8000) > 0;
        set
        {
            if (value)
            {
                Flags |= 0x8000;
            }
            else
            {
                Flags = (ushort)(Flags & ~0x8000);
            }
        }
    }

    public bool Invisible
    {
        get => (Flags & 0x100) > 0;
        set
        {
            if (value)
            {
                Flags |= 0x100;
            }
            else
            {
                Flags = (ushort)(Flags & ~0x100);
            }
        }
    }

    public ushort CodeBits
    {
        get => (ushort)((Flags & 0x3E00) >> 9);
        set
        {
            Flags = (ushort)(Flags & ~(Flags & 0x3E00));
            Flags |= (ushort)(value << 9);
        }
    }

    public override string ToString()
    {
        return $"{base.ToString()} TypeID: {TypeID} X: {X} Y: {Y} Z: {Z} Room: {Room} Angle: {Angle} Flags: {Flags}";
    }
}
