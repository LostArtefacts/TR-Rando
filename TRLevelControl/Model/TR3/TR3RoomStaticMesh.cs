namespace TRLevelControl.Model;

public class TR3RoomStaticMesh : TRRoomStaticMesh<TR3Type>, ICloneable
{
    public ushort Colour { get; set; }
    public ushort Unused { get; set; }

    public TR3RoomStaticMesh Clone()
        => (TR3RoomStaticMesh)MemberwiseClone();

    object ICloneable.Clone()
        => Clone();
}
