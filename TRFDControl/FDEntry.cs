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
            CameraActionList = new List<FDCameraAction>();
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

        public List<FDCameraAction> CameraActionList { get; set; }

        public ushort SwitchOrKeyRef { get; set; }
    }
}
