namespace TRLevelControlTests;

public class TR2Observer : TR1Observer
{
    private readonly Dictionary<int, Dictionary<int, List<short>>> _framePadding = new();

    public override void OnFramePaddingRead(int animIndex, int frameIndex, List<short> values)
    {
        if (!_framePadding.ContainsKey(animIndex))
        {
            _framePadding[animIndex] = new();
        }
        _framePadding[animIndex][frameIndex] = values;
    }

    public override List<short> GetFramePadding(int animIndex, int frameIndex)
    {
        if (_framePadding.ContainsKey(animIndex)
            && _framePadding[animIndex].ContainsKey(frameIndex))
        {
            return _framePadding[animIndex][frameIndex];
        }
        return null;
    }
}
