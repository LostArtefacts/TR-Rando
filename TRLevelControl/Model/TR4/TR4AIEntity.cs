namespace TRLevelControl.Model;

public class TR4AIEntity : TREntity<TR4Type>
{
    public short OCB { get; set; }
    public short Box { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()} OCB: {OCB} Box: {Box}";
    }
}
