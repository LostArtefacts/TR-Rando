using System;

namespace TRRandomizerCore.Textures
{
    public class TRSize : IComparable<TRSize>
    {
        public int W { get; set; }
        public int H { get; set; }
        public int Area => W * H;

        public TRSize(int w, int h)
        {
            W = w;
            H = h;
        }

        public int CompareTo(TRSize other)
        {
            if (W == other.W)
            {
                return H.CompareTo(other.H);
            }
            return W.CompareTo(other.W);
        }

        public override bool Equals(object obj)
        {
            return obj is TRSize size && size.W == W && size.H == H;
        }

        public override int GetHashCode()
        {
            return 124555757 + W.GetHashCode() + H.GetHashCode();
        }

        public override string ToString()
        {
            return "W: " + W + ", H: " + H + ", A: " + Area;
        }
    }
}