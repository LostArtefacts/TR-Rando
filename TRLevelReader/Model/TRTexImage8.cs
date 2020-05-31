using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRTexImage8
    {
        public byte[] Pixels { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append("\n");

            int Count = 1;
            foreach (byte pixel in Pixels)
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
