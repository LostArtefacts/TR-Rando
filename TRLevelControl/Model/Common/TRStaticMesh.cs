namespace TRLevelControl.Model;

public class TRStaticMesh : ICloneable
{
    public uint ID { get; set; }
    public TRMesh Mesh { get; set; }
    public TRBoundingBox VisibilityBox { get; set; }
    public TRBoundingBox CollisionBox { get; set; }
    public ushort Flags { get; set; }

    public bool NonCollidable
    {
        get => (Flags & 1) > 0;
        set
        {
            if (value)
            {
                Flags |= 1;
            }
            else
            {
                Flags ^= 1;
            }
        }
    }

    public bool Visible
    {
        get => (Flags & 2) > 0;
        set
        {
            if (value)
            {
                Flags |= 2;
            }
            else
            {
                Flags ^= 2;
            }
        }
    }

    public TRStaticMesh Clone()
    {
        return new()
        {
            ID = ID,
            Mesh = Mesh.Clone(),
            VisibilityBox = VisibilityBox.Clone(),
            CollisionBox = CollisionBox.Clone(),
            Flags = Flags
        };
    }

    object ICloneable.Clone()
        => Clone();
}
