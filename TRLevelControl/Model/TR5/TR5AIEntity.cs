namespace TRLevelControl.Model;

public class TR5AIEntity : TREntity<TR5Type>
{
    public short OCB { get; set; }
    public short Box { get; set; }

    public override string ToString()
    {
        return $"{base.ToString()} OCB: {OCB} Box: {Box}";
    }
}
