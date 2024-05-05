namespace TRImageControl.Helpers;

public struct HSB
{
    public int H { get; set; }
    public int S { get; set; }
    public int B { get; set; }
    public int A { get; set; }

    public HSB(int h, int s, int b, int a)
    {
        H = h;
        S = s;
        B = b;
        A = a;
    }
}
