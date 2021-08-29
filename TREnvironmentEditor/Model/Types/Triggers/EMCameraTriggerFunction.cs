using System.Collections.Generic;
using System.Linq;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMCameraTriggerFunction : BaseEMFunction
    {
        public TRCamera Camera { get; set; }
        public ushort LookAtItem { get; set; }
        public ushort AttachToItem { get; set; }
        public FDCameraAction CameraAction { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            List<TRCamera> cameras = level.Cameras.ToList();
            cameras.Add(Camera);
            level.Cameras = cameras.ToArray();
            level.NumCameras++;

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TR2Entity attachToEntity = level.Entities[AttachToItem];
            TRRoomSector sector = FDUtilities.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, attachToEntity.Room, level, control);
            if (sector.FDIndex != 0)
            {
                FDTriggerEntry trigger = control.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
                if (trigger != null)
                {
                    trigger.TrigActionList.Insert(0, new FDActionListItem
                    {
                        TrigAction = FDTrigAction.Camera,
                        Value = 1024,
                        CamAction = CameraAction,
                        Parameter = (ushort)(cameras.Count - 1)
                    });
                    trigger.TrigActionList.Insert(1, new FDActionListItem
                    {
                        TrigAction = FDTrigAction.LookAtItem,
                        Value = 6158,
                        Parameter = LookAtItem
                    });

                    control.WriteToLevel(level);
                }
            }
        }
    }
}