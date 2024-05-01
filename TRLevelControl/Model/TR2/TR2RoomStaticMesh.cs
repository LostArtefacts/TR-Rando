namespace TRLevelControl.Model;

public class TR2RoomStaticMesh : TRRoomStaticMesh<TR2Type>, ICloneable
{
    public ushort Intensity1 { get; set; }
    public ushort Intensity2 { get; set; }

    public TR2RoomStaticMesh Clone()
        => (TR2RoomStaticMesh)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
