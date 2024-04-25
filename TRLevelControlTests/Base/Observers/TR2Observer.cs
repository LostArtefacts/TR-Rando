namespace TRLevelControlTests;

public class TR2Observer : TR1Observer
{
    private readonly Dictionary<int, Tuple<ushort, ushort>> _animLinks = new();

    public override void OnBadAnimLinkRead(int animIndex, ushort animLink, ushort frameLink)
    {
        _animLinks[animIndex] = new(animLink, frameLink);
    }

    public override Tuple<ushort, ushort> GetAnimLink(int animIndex)
    {
        return _animLinks.ContainsKey(animIndex) ? _animLinks[animIndex] : null;
    }
}
