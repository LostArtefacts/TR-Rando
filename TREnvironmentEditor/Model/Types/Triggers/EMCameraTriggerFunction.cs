using System;
using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
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
        public EMLocation AttachToLocation { get; set; }
        public FDCameraAction CameraAction { get; set; }

        public EMCameraTriggerFunction()
        {
            LookAtItem = ushort.MaxValue;
            AttachToItem = ushort.MaxValue;
        }

        public override void ApplyToLevel(TR2Level level)
        {
            List<TRCamera> cameras = level.Cameras.ToList();
            cameras.Add(Camera);
            level.Cameras = cameras.ToArray();
            level.NumCameras++;

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector sector = null;
            if (AttachToItem != ushort.MaxValue)
            {
                TR2Entity attachToEntity = level.Entities[AttachToItem];
                sector = FDUtilities.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, attachToEntity.Room, level, control);
            }
            else if (AttachToLocation != null)
            {
                sector = FDUtilities.GetRoomSector(AttachToLocation.X, AttachToLocation.Y, AttachToLocation.Z, AttachToLocation.Room, level, control);
            }
            else
            {
                throw new ArgumentException("Cannot attach camera to an undefined trigger location.");
            }
            
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

                    if (LookAtItem != ushort.MaxValue)
                    {
                        trigger.TrigActionList.Insert(1, new FDActionListItem
                        {
                            TrigAction = FDTrigAction.LookAtItem,
                            Value = 6158,
                            Parameter = LookAtItem
                        });
                    }

                    control.WriteToLevel(level);
                }
            }
        }
    }
}