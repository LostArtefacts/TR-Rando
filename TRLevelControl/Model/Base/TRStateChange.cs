using System.Text;

namespace TRLevelControl.Model;

public class TRStateChange
{
    public ushort StateID { get; set; }

    public ushort NumAnimDispatches { get; set; }

    public ushort AnimDispatch { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" StateID: " + StateID);
        sb.Append(" NumAnimDispatches: " + NumAnimDispatches);
        sb.Append(" AnimDispatch: " + AnimDispatch);

        return sb.ToString();
    }
}
