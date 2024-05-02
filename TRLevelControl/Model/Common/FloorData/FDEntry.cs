namespace TRLevelControl.Model;

public abstract class FDEntry : ICloneable
{
    public abstract FDFunction GetFunction();
    public abstract FDEntry Clone();

    object ICloneable.Clone()
        => Clone();
}
