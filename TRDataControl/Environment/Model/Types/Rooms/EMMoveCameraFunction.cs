﻿using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMMoveCameraFunction : BaseEMFunction
{
    public int CameraIndex { get; set; }
    public EMLocation NewLocation { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        EMLevelData data = GetData(level);
        MoveCamera(level.Cameras[data.ConvertCamera(CameraIndex)], data);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        EMLevelData data = GetData(level);
        MoveCamera(level.Cameras[data.ConvertCamera(CameraIndex)], data);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        EMLevelData data = GetData(level);
        MoveCamera(level.Cameras[data.ConvertCamera(CameraIndex)], data);
    }

    private void MoveCamera(TRCamera camera, EMLevelData data)
    {
        camera.X = NewLocation.X;
        camera.Y = NewLocation.Y;
        camera.Z = NewLocation.Z;
        camera.Room = data.ConvertRoom(NewLocation.Room);
    }
}
