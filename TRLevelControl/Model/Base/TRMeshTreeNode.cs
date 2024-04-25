namespace TRLevelControl.Model;

public class TRMeshTreeNode : ICloneable
{
    public uint Flags { get; set; }
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
    public int OffsetZ { get; set; }

    public TRMeshTreeNode Clone()
    {
        return new()
        {
            Flags = Flags,
            OffsetX = OffsetX,
            OffsetY = OffsetY,
            OffsetZ = OffsetZ
        };
    }

    object ICloneable.Clone()
        => Clone();
}
