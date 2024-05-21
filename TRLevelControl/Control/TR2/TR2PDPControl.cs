using TRLevelControl.Build;
using TRLevelControl.Model;

namespace TRLevelControl;

public class TR2PDPControl : TRPDPControlBase<TR2Type>
{
    public TR2PDPControl(ITRLevelObserver observer = null)
        : base(observer) { }

    protected override TRModelBuilder<TR2Type> CreateBuilder()
        => new(TRGameVersion.TR2, TRModelDataType.PDP);
}
