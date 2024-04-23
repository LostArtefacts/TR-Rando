using System.Text;

namespace TRLevelControl.Model;

public class TRAnimCommand
{
    public short Value { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());

        sb.Append(" Value: " + Value.ToString("X4"));

        return sb.ToString();
    }
}
