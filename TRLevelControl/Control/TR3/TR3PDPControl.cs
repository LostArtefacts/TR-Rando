using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR3PDPControl : TRPDPControlBase<TR3Type>
{
    public TR3PDPControl(ITRLevelObserver observer = null)
        : base(observer) { }

    protected override TRModelBuilder<TR3Type> CreateBuilder()
        => new(TRGameVersion.TR3, TRModelDataType.PDP);
}
