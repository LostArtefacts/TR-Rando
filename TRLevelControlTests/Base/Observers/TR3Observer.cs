namespace TRLevelControlTests;

public class TR3Observer : TR2Observer
{
    private readonly Dictionary<int, Tuple<ushort, ushort>> _animLinks = new();
    private readonly bool _useOriginalFloorData;
    private ushort? _badOverlap;
    private ushort[] _floorData;

    public TR3Observer(bool useOriginalFD = false)
    {
        _useOriginalFloorData = useOriginalFD;
    }

    public override bool UseOriginalFloorData => _useOriginalFloorData;

    public override void OnFloorDataRead(ushort[] data)
    {
        _floorData = data;
    }

    public override ushort[] GetFloorData() =>
        _useOriginalFloorData ? _floorData : null;

    public override void OnBadAnimLinkRead(int animIndex, ushort animLink, ushort frameLink)
    {
        _animLinks[animIndex] = new(animLink, frameLink);
    }

    public override Tuple<ushort, ushort> GetAnimLink(int animIndex)
    {
        return _animLinks.ContainsKey(animIndex) ? _animLinks[animIndex] : null;
    }

    public override void OnBadOverlapRead(ushort value)
    {
        _badOverlap = value;
    }

    public override ushort? GetBadOverlap()
    {
        return _badOverlap;
    }
}
