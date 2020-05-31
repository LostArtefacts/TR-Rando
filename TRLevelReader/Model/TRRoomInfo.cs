using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRLevelReader.Model
{
    public class TRRoomInfo
    {
        public int X { get; set; }

        public int Z { get; set; }

        public int YBottom { get; set; }

        public int YTop { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(base.ToString());

            sb.Append(" X: " + X);
            sb.Append(" Z: " + Z);
            sb.Append(" YBottom: " + YBottom);
            sb.Append(" YTop: " + YTop);

            return sb.ToString();
        }
    }
}
