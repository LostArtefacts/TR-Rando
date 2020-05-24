using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR2Randomizer
{
    public class TRItem
    {
        public int ID { get; set; }
        
        public int OID { get; set; }

        public Location Location { get; set; }

        public string Params { get; set; }

        public int Angle { get; set; }

        public int Intensity1 { get; set; }

        public int Intensity2 { get; set; }

        public TRItem()
        {
            Params = " 0 -1 -1 0000";
        }

        public string TRMODReplaceCommand
        {
            get
            {
                StringBuilder cmd = new StringBuilder(" REPLACE ITEM ");

                cmd.Append(ID + " ");
                cmd.Append(OID + " ");
                cmd.Append(Location.Room + " ");
                cmd.Append(Location.X + " ");
                cmd.Append(Location.Z + " ");
                cmd.Append(Location.Y);
                cmd.Append(Params);

                return cmd.ToString();
            }
        }

        public string TRMODAddCommand
        {
            get
            {
                StringBuilder cmd = new StringBuilder(" ADD ITEM ");

                cmd.Append(OID + " ");
                cmd.Append(Location.Room + " ");
                cmd.Append(Location.X + " ");
                cmd.Append(Location.Z + " ");
                cmd.Append(Location.Y + " ");
                cmd.Append(Angle + " ");
                cmd.Append(Intensity1 + " ");
                cmd.Append(Intensity2);

                cmd.Append(Params);

                return cmd.ToString();
            }
        }
    }
}
