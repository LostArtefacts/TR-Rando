namespace TRLevelControl.Model;

public class TRDemoData<G, I>
    where G : Enum
    where I : Enum
{
    public TRVertex32 LaraPos { get; set; }
    public TRVertex32 LaraRot { get; set; }
    public int LaraRoom { get; set; }
    public G LaraLastGun { get; set; }
    public List<I> Inputs { get; set; } = new();
}
