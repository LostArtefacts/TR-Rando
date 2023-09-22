using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMCameraTriggerFunction : BaseEMFunction
{
    public short CameraIndex { get; set; }
    public TRCamera Camera { get; set; }
    public int LookAtItem { get; set; }
    public int[] AttachToItems { get; set; }
    public EMLocation[] AttachToLocations { get; set; }
    public short[] AttachToRooms { get; set; }
    public FDCameraAction CameraAction { get; set; }

    public EMCameraTriggerFunction()
    {
        LookAtItem = ushort.MaxValue;
    }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        ushort cameraIndex;
        if (Camera != null)
        {
            List<TRCamera> cameras = level.Cameras.ToList();
            cameras.Add(CreateCamera(data));
            level.Cameras = cameras.ToArray();

            cameraIndex = (ushort)level.NumCameras;
            level.NumCameras++;
        }
        else
        {
            cameraIndex = (ushort)data.ConvertCamera(CameraIndex);
        }

        FDControl control = new();
        control.ParseFromLevel(level);

        if (AttachToItems != null)
        {
            foreach (int item in AttachToItems)
            {
                TR1Entity attachToEntity = level.Entities[data.ConvertEntity(item)];
                TRRoomSector sector = FDUtilities.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, data.ConvertRoom(attachToEntity.Room), level, control);
                AttachToSector(sector, control, cameraIndex, data);
            }
        }

        if (AttachToLocations != null)
        {
            foreach (EMLocation location in AttachToLocations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                AttachToSector(sector, control, cameraIndex, data);
            }
        }

        if (AttachToRooms != null)
        {
            foreach (short room in AttachToRooms)
            {
                foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                {
                    AttachToSector(sector, control, cameraIndex, data);
                }
            }
        }

        control.WriteToLevel(level);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        ushort cameraIndex;
        if (Camera != null)
        {
            List<TRCamera> cameras = level.Cameras.ToList();
            cameras.Add(CreateCamera(data));
            level.Cameras = cameras.ToArray();

            cameraIndex = (ushort)level.NumCameras;
            level.NumCameras++;
        }
        else
        {
            cameraIndex = (ushort)data.ConvertCamera(CameraIndex);
        }

        FDControl control = new();
        control.ParseFromLevel(level);

        if (AttachToItems != null)
        {
            foreach (int item in AttachToItems)
            {
                TR2Entity attachToEntity = level.Entities[data.ConvertEntity(item)];
                TRRoomSector sector = FDUtilities.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, data.ConvertRoom(attachToEntity.Room), level, control);
                AttachToSector(sector, control, cameraIndex, data);
            }
        }

        if (AttachToLocations != null)
        {
            foreach (EMLocation location in AttachToLocations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                AttachToSector(sector, control, cameraIndex, data);
            }
        }

        if (AttachToRooms != null)
        {
            foreach (short room in AttachToRooms)
            {
                foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].SectorList)
                {
                    AttachToSector(sector, control, cameraIndex, data);
                }
            }
        }

        control.WriteToLevel(level);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        ushort cameraIndex;
        if (Camera != null)
        {
            List<TRCamera> cameras = level.Cameras.ToList();
            cameras.Add(CreateCamera(data));
            level.Cameras = cameras.ToArray();

            cameraIndex = (ushort)level.NumCameras;
            level.NumCameras++;
        }
        else
        {
            cameraIndex = (ushort)data.ConvertCamera(CameraIndex);
        }

        FDControl control = new();
        control.ParseFromLevel(level);

        if (AttachToItems != null)
        {
            foreach (int item in AttachToItems)
            {
                TR3Entity attachToEntity = level.Entities[data.ConvertEntity(item)];
                TRRoomSector sector = FDUtilities.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, data.ConvertRoom(attachToEntity.Room), level, control);
                AttachToSector(sector, control, cameraIndex, data);
            }
        }

        if (AttachToLocations != null)
        {
            foreach (EMLocation location in AttachToLocations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
                AttachToSector(sector, control, cameraIndex, data);
            }
        }

        if (AttachToRooms != null)
        {
            foreach (short room in AttachToRooms)
            {
                foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                {
                    AttachToSector(sector, control, cameraIndex, data);
                }
            }
        }

        control.WriteToLevel(level);
    }

    private TRCamera CreateCamera(EMLevelData data)
    {
        return new TRCamera
        {
            X = Camera.X,
            Y = Camera.Y,
            Z = Camera.Z,
            Room = data.ConvertRoom(Camera.Room),
            Flag = Camera.Flag,
        };
    }

    private void AttachToSector(TRRoomSector sector, FDControl control, ushort cameraIndex, EMLevelData data)
    {
        if (sector.FDIndex != 0)
        {
            if (control.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                && trigger.TrigType != FDTrigType.Dummy)
            {
                trigger.TrigActionList.Add(new FDActionListItem
                {
                    TrigAction = FDTrigAction.Camera,
                    Value = 1024,
                    CamAction = CameraAction,
                    Parameter = cameraIndex
                });

                if (LookAtItem != ushort.MaxValue)
                {
                    trigger.TrigActionList.Add(new FDActionListItem
                    {
                        TrigAction = FDTrigAction.LookAtItem,
                        Value = 6158,
                        Parameter = (ushort)data.ConvertEntity(LookAtItem)
                    });
                }
            }
        }
    }
}
