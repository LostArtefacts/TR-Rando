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
        public ushort CameraIndex { get; set; }
        public TRCamera Camera { get; set; }
        public ushort LookAtItem { get; set; }
        public ushort[] AttachToItems { get; set; }
        public EMLocation[] AttachToLocations { get; set; }
        public short[] AttachToRooms { get; set; }
        public FDCameraAction CameraAction { get; set; }

        public EMCameraTriggerFunction()
        {
            LookAtItem = ushort.MaxValue;
        }

        public override void ApplyToLevel(TRLevel level)
        {
            ushort cameraIndex;
            if (Camera != null)
            {
                List<TRCamera> cameras = level.Cameras.ToList();
                cameras.Add(Camera);
                level.Cameras = cameras.ToArray();

                cameraIndex = (ushort)level.NumCameras;
                level.NumCameras++;
            }
            else
            {
                cameraIndex = CameraIndex;
            }

            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            if (AttachToItems != null)
            {
                foreach (ushort item in AttachToItems)
                {
                    TREntity attachToEntity = level.Entities[item];
                    TRRoomSector sector = FDUtilities.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, data.ConvertRoom(attachToEntity.Room), level, control);
                    AttachToSector(sector, control, cameraIndex);
                }
            }

            if (AttachToLocations != null)
            {
                foreach (EMLocation location in AttachToLocations)
                {
                    TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                    AttachToSector(sector, control, cameraIndex);
                }
            }

            if (AttachToRooms != null)
            {
                foreach (short room in AttachToRooms)
                {
                    foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                    {
                        AttachToSector(sector, control, cameraIndex);
                    }
                }
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR2Level level)
        {
            ushort cameraIndex;
            if (Camera != null)
            {
                List<TRCamera> cameras = level.Cameras.ToList();
                cameras.Add(Camera);
                level.Cameras = cameras.ToArray();

                cameraIndex = (ushort)level.NumCameras;
                level.NumCameras++;
            }
            else
            {
                cameraIndex = CameraIndex;
            }

            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            if (AttachToItems != null)
            {
                foreach (ushort item in AttachToItems)
                {
                    TR2Entity attachToEntity = level.Entities[item];
                    TRRoomSector sector = FDUtilities.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, data.ConvertRoom(attachToEntity.Room), level, control);
                    AttachToSector(sector, control, cameraIndex);
                }
            }

            if (AttachToLocations != null)
            {
                foreach (EMLocation location in AttachToLocations)
                {
                    TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                    AttachToSector(sector, control, cameraIndex);
                }
            }

            if (AttachToRooms != null)
            {
                foreach (short room in AttachToRooms)
                {
                    foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].SectorList)
                    {
                        AttachToSector(sector, control, cameraIndex);
                    }
                }
            }

            control.WriteToLevel(level);
        }

        public override void ApplyToLevel(TR3Level level)
        {
            ushort cameraIndex;
            if (Camera != null)
            {
                List<TRCamera> cameras = level.Cameras.ToList();
                cameras.Add(Camera);
                level.Cameras = cameras.ToArray();

                cameraIndex = (ushort)level.NumCameras;
                level.NumCameras++;
            }
            else
            {
                cameraIndex = CameraIndex;
            }

            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            if (AttachToItems != null)
            {
                foreach (ushort item in AttachToItems)
                {
                    TR2Entity attachToEntity = level.Entities[item];
                    TRRoomSector sector = FDUtilities.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, data.ConvertRoom(attachToEntity.Room), level, control);
                    AttachToSector(sector, control, cameraIndex);
                }
            }

            if (AttachToLocations != null)
            {
                foreach (EMLocation location in AttachToLocations)
                {
                    TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                    AttachToSector(sector, control, cameraIndex);
                }
            }

            if (AttachToRooms != null)
            {
                foreach (short room in AttachToRooms)
                {
                    foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                    {
                        AttachToSector(sector, control, cameraIndex);
                    }
                }
            }

            control.WriteToLevel(level);
        }

        private void AttachToSector(TRRoomSector sector, FDControl control, ushort cameraIndex)
        {
            if (sector.FDIndex != 0)
            {
                FDTriggerEntry trigger = control.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
                if (trigger != null && trigger.TrigType != FDTrigType.Dummy)
                {
                    trigger.TrigActionList.Add(/*0, */new FDActionListItem
                    {
                        TrigAction = FDTrigAction.Camera,
                        Value = 1024,
                        CamAction = CameraAction,
                        Parameter = cameraIndex
                    });

                    if (LookAtItem != ushort.MaxValue)
                    {
                        trigger.TrigActionList.Add(/*1, */new FDActionListItem
                        {
                            TrigAction = FDTrigAction.LookAtItem,
                            Value = 6158,
                            Parameter = LookAtItem
                        });
                    }
                }
            }
        }
    }
}