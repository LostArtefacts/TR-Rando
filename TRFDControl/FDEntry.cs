using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl
{
    public class FDEntry
    {
        public FDSetup Setup { get; set; }

        public virtual ushort[] Flatten()
        {
            return new ushort[] { Setup.Value };
        }
    }

    public class FDPortalEntry : FDEntry
    {
        public ushort Room { get; set; }

        public override ushort[] Flatten()
        {
            return new ushort[]
            {
                Setup.Value,
                Room
            };
        }
    }

    public class FDClimbEntry : FDEntry
    {
        public bool IsPositiveX
        {
            get
            {
                return ((Setup.SubFunction & (byte)FDClimbDirection.PositiveX) > 0);
            }
        }

        public bool IsPositiveZ
        {
            get
            {
                return ((Setup.SubFunction & (byte)FDClimbDirection.PositiveZ) > 0);
            }
        }

        public bool IsNegativeX
        {
            get
            {
                return ((Setup.SubFunction & (byte)FDClimbDirection.NegativeX) > 0);
            }
        }

        public bool IsNegativeZ
        {
            get
            {
                return ((Setup.SubFunction & (byte)FDClimbDirection.NegativeZ) > 0);
            }
        }
    }

    public class FDKillLaraEntry : FDEntry
    {

    }

    public class FDTriggerEntry : FDEntry
    {
        public FDTriggerEntry()
        {
            TrigActionList = new List<FDActionListItem>();
        }

        public FDTrigSetup TrigSetup { get; set; }

        public FDTrigType TrigType
        {
            get
            {
                return (FDTrigType)Setup.SubFunction;
            }
        }

        public List<FDActionListItem> TrigActionList { get; set; }

        public ushort SwitchOrKeyRef { get; set; }

        public override ushort[] Flatten()
        {
            //FD Setup followed by TrigSetup
            List<ushort> dataArray = new List<ushort>
            {
                Setup.Value,
                TrigSetup.Value
            };

            //If key or switch, next uint16 will be ref to key or switch ent
            if (TrigType == FDTrigType.Switch || TrigType == FDTrigType.Key)
                dataArray.Add(SwitchOrKeyRef);


            foreach (FDActionListItem action in TrigActionList)
            {
                //For each action, record the value.
                dataArray.Add(action.Value);

                //If it is a camera action, record the additional camera action
                if (action.TrigAction == FDTrigAction.Camera)
                {
                    dataArray.Add(action.CamAction.Value);
                }
            }

            //Return uint16 array
            return dataArray.ToArray();
        }
    }
}
