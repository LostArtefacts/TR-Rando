namespace TRDataControl.Environment;

public class BaseEMObject
{
    protected bool _isCommunityPatch, _isRemastered;

    public void SetCommunityPatch(bool isCommunityPatch)
        => _isCommunityPatch = isCommunityPatch;

    public void SetRemastered(bool isRemasterd)
        => _isRemastered = isRemasterd;
}
