namespace TRLevelControl.Model;

public class TR1RoomStaticMesh : TRRoomStaticMesh<TR1Type>, ICloneable
{
    public ushort Intensity { get; set; }

    public TR1RoomStaticMesh Clone()
        => (TR1RoomStaticMesh)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
