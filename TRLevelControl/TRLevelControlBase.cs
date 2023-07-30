using TRLevelControl.Model;

namespace TRLevelControl;

public abstract class TRLevelControlBase<L>
    where L : TRLevelBase
{
    protected void TestVersion(L level, params TRFileVersion[] acceptedVersions)
    {
        if (!acceptedVersions.Contains(level.Version.File))
        {
            throw new NotSupportedException($"Unexpected level version: {level.Version.File} ({(uint)level.Version.File})");
        }
    }
}
