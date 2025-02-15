namespace TRLevelControlTests;

public class TR5Observer : TR4Observer
{
    private Tuple<ushort, ushort> _unusedStateChange;
    private short? _animCommandPadding;
    private List<byte> _rawRooms;

    public TR5Observer(bool remastered)
        : base(remastered) { }

    public override bool UseTR5RawRooms => true;

    public override void OnRawTR5RoomsRead(List<byte> data)
    {
        _rawRooms = data;
    }

    public override List<byte> GetTR5Rooms()
    {
        return _rawRooms;
    }

    public override void OnAnimCommandPaddingRead(short padding)
    {
        _animCommandPadding = padding;
    }

    public override short? GetAnimCommandPadding()
    {
        return _animCommandPadding;
    }

    public override void OnUnusedStateChangeRead(Tuple<ushort, ushort> changePadding)
    {
        _unusedStateChange = changePadding;
    }

    public override Tuple<ushort, ushort> GetUnusedStateChange()
    {
        return _unusedStateChange;
    }
}
