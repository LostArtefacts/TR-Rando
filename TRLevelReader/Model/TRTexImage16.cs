using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRTexImage16
    {
        public ushort[] Pixels { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append("\n");

            int Count = 1;
            foreach (ushort pixel in Pixels)
            {
                sb.Append(pixel + " ");

                Count++;

                if (Count % 8 == 0)
                {
                    sb.Append("\n");
                }
            }

            return sb.ToString();
        }
    }
}
