using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR1FileReadUtilities
{
    public static TR1RoomLight ReadRoomLight(BinaryReader reader)
    {
        return new()
        {
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            Intensity = reader.ReadUInt16(),
            Fade = reader.ReadUInt32(),
        };
    }

    public static TR1RoomStaticMesh ReadRoomStaticMesh(BinaryReader reader)
    {
        return new()
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
        return new()
        {
            ZMin = reader.ReadUInt32(),
            ZMax = reader.ReadUInt32(),
            XMin = reader.ReadUInt32(),
            XMax = reader.ReadUInt32(),
            TrueFloor = reader.ReadInt16(),
            OverlapIndex = reader.ReadUInt16()
        };
    }
}
