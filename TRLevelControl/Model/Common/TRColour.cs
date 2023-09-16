namespace TRLevelControl.Model;

public class TRColour
{
    public byte Red { get; set; }
    public byte Green { get; set; }    
    public byte Blue { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()} R: {Red} G: {Green} B: {Blue}";
    }
}
