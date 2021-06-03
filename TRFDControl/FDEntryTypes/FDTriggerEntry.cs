using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRFDControl.FDEntryTypes
{
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
