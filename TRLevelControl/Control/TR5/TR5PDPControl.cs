using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR5PDPControl : TRPDPControlBase<TR5Type>
{
    public TR5PDPControl(ITRLevelObserver observer = null)
        : base(observer) { }

    protected override TRModelBuilder<TR5Type> CreateBuilder()
        => new(TRGameVersion.TR5, TRModelDataType.PDP, null, true);
}
