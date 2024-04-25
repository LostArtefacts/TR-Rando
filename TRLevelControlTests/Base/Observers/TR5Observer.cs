namespace TRLevelControlTests;

public class TR5Observer : TR4Observer
{
    private Tuple<ushort, ushort> _unusedStateChange;
    private short? _animCommandPadding;

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
