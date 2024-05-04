using TRLevelControl.Model;

namespace TRDataControl.Environment;

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
        short cameraIndex;
        if (Camera != null)
        {
            cameraIndex = (short)level.Cameras.Count;
            level.Cameras.Add(CreateCamera(data));
        }
        else
        {
            cameraIndex = data.ConvertCamera(CameraIndex);
        }

        if (AttachToItems != null)
        {
            foreach (int item in AttachToItems)
            {
                TR1Entity attachToEntity = level.Entities[data.ConvertEntity(item)];
                TRRoomSector sector = level.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, data.ConvertRoom(attachToEntity.Room));
                AttachToSector(sector, level.FloorData, cameraIndex, data);
            }
        }

        if (AttachToLocations != null)
        {
            foreach (EMLocation location in AttachToLocations)
            {
                TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
                AttachToSector(sector, level.FloorData, cameraIndex, data);
            }
        }

        if (AttachToRooms != null)
        {
            foreach (short room in AttachToRooms)
            {
                foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                {
                    AttachToSector(sector, level.FloorData, cameraIndex, data);
                }
            }
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        short cameraIndex;
        if (Camera != null)
        {
            cameraIndex = (short)level.Cameras.Count;
            level.Cameras.Add(CreateCamera(data));
        }
        else
        {
            cameraIndex = data.ConvertCamera(CameraIndex);
        }

        if (AttachToItems != null)
        {
            foreach (int item in AttachToItems)
            {
                TR2Entity attachToEntity = level.Entities[data.ConvertEntity(item)];
                TRRoomSector sector = level.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, data.ConvertRoom(attachToEntity.Room));
                AttachToSector(sector, level.FloorData, cameraIndex, data);
            }
        }

        if (AttachToLocations != null)
        {
            foreach (EMLocation location in AttachToLocations)
            {
                TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
                AttachToSector(sector, level.FloorData, cameraIndex, data);
            }
        }

        if (AttachToRooms != null)
        {
            foreach (short room in AttachToRooms)
            {
                foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                {
                    AttachToSector(sector, level.FloorData, cameraIndex, data);
                }
            }
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        short cameraIndex;
        if (Camera != null)
        {
            cameraIndex = (short)level.Cameras.Count;
            level.Cameras.Add(CreateCamera(data));
        }
        else
        {
            cameraIndex = data.ConvertCamera(CameraIndex);
        }

        if (AttachToItems != null)
        {
            foreach (int item in AttachToItems)
            {
                TR3Entity attachToEntity = level.Entities[data.ConvertEntity(item)];
                TRRoomSector sector = level.GetRoomSector(attachToEntity.X, attachToEntity.Y, attachToEntity.Z, data.ConvertRoom(attachToEntity.Room));
                AttachToSector(sector, level.FloorData, cameraIndex, data);
            }
        }

        if (AttachToLocations != null)
        {
            foreach (EMLocation location in AttachToLocations)
            {
                TRRoomSector sector = level.GetRoomSector(data.ConvertLocation(location));
                AttachToSector(sector, level.FloorData, cameraIndex, data);
            }
        }

        if (AttachToRooms != null)
        {
            foreach (short room in AttachToRooms)
            {
                foreach (TRRoomSector sector in level.Rooms[data.ConvertRoom(room)].Sectors)
                {
                    AttachToSector(sector, level.FloorData, cameraIndex, data);
                }
            }
        }
    }

    private TRCamera CreateCamera(EMLevelData data)
    {
        return new()
        {
            X = Camera.X,
            Y = Camera.Y,
            Z = Camera.Z,
            Room = data.ConvertRoom(Camera.Room),
            Flag = Camera.Flag,
        };
    }

    private void AttachToSector(TRRoomSector sector, FDControl control, short cameraIndex, EMLevelData data)
    {
        if (sector.FDIndex != 0)
        {
            if (control[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger
                && trigger.TrigType != FDTrigType.Dummy)
            {
                trigger.Actions.Add(new()
                {
                    Action = FDTrigAction.Camera,
                    CamAction = CameraAction,
                    Parameter = cameraIndex
                });

                if (LookAtItem != ushort.MaxValue)
                {
                    trigger.Actions.Add(new()
                    {
                        Action = FDTrigAction.LookAtItem,
                        Parameter = data.ConvertEntity(LookAtItem)
                    });
                }
            }
        }
    }
}
