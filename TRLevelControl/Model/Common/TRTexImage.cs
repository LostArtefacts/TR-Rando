using System.Text;

namespace TRLevelControl.Model;

public class TRTexImage<T>
{
    public T[] Pixels { get; set; }

    public override string ToString()
    {
        StringBuilder sb = new(base.ToString());
        for (int i = 0; i < Pixels.Length; i++)
        {
            if (i % 8 == 0)
            {
                sb.Append('\n');
            }
            sb.Append(Pixels[i] + " ");
        }

        return sb.ToString();
    }
}
