using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR1PDPControl : TRPDPControlBase<TR1Type>
{
    public TR1PDPControl(ITRLevelObserver observer = null)
        : base(observer) { }

    protected override TRModelBuilder<TR1Type> CreateBuilder()
        => new(TRGameVersion.TR1, TRModelDataType.PDP);
}
