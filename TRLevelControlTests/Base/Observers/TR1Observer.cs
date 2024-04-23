namespace TRLevelControlTests;

public class TR1Observer : ObserverBase
{
    private readonly Dictionary<int, Tuple<short, short>> _dispatchLinks = new();

    public override void OnBadDispatchLinkRead(int dispatchIndex, short animLink, short frameLink)
    {
        _dispatchLinks[dispatchIndex] = new(animLink, frameLink);
    }

    public override Tuple<short, short> GetDispatchLink(int dispatchIndex)
    {
        return _dispatchLinks.ContainsKey(dispatchIndex) ? _dispatchLinks[dispatchIndex] : null;
    }
}
