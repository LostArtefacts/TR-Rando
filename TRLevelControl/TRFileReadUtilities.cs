using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TRFileReadUtilities
{
    public static TRRoomLight ReadRoomLight(BinaryReader reader)
    {
        return new TRRoomLight
        {
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            Intensity = reader.ReadUInt16(),
            Fade = reader.ReadUInt32(),
        };
    }

    public static TRRoomStaticMesh ReadRoomStaticMesh(BinaryReader reader)
    {
        return new TRRoomStaticMesh
        {
            X = reader.ReadUInt32(),
            Y = reader.ReadUInt32(),
            Z = reader.ReadUInt32(),
            Rotation = reader.ReadUInt16(),
            Intensity = reader.ReadUInt16(),
            MeshID = reader.ReadUInt16()
        };
    }

    public static TRBox ReadBox(BinaryReader reader)
    {
        return new TRBox()
        {
            ZMin = reader.ReadUInt32(),
            ZMax = reader.ReadUInt32(),
            XMin = reader.ReadUInt32(),
            XMax = reader.ReadUInt32(),
            TrueFloor = reader.ReadInt16(),
            OverlapIndex = reader.ReadUInt16()
        };
    }

    public static TREntity ReadEntity(BinaryReader reader)
    {
        return new TREntity()
        {
            TypeID = reader.ReadInt16(),
            Room = reader.ReadInt16(),
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            Angle = reader.ReadInt16(),
            Intensity = reader.ReadInt16(),
            Flags = reader.ReadUInt16()
        };
    }
}
