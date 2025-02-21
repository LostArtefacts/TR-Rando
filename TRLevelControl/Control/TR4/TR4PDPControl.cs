using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR4PDPControl : TRPDPControlBase<TR4Type>
{
    public TR4PDPControl(ITRLevelObserver observer = null)
        : base(observer) { }

    protected override TRModelBuilder<TR4Type> CreateBuilder()
        => new(TRGameVersion.TR4, TRModelDataType.PDP, null, true);
}
