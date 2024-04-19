using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR3FileReadUtilities
{
    public static TR3RoomLight ReadRoomLight(BinaryReader reader)
    {
        return new TR3RoomLight
        {
            X = reader.ReadInt32(),
            Y = reader.ReadInt32(),
            Z = reader.ReadInt32(),
            Colour = new TRColour
            {
                Red = reader.ReadByte(),
                Green = reader.ReadByte(),
                Blue = reader.ReadByte()
            },
            LightType = reader.ReadByte(),
            LightProperties = new short[4]
            {
                reader.ReadInt16(),
                reader.ReadInt16(),
                reader.ReadInt16(),
                reader.ReadInt16()
            }
        };
    }

    public static TR3RoomStaticMesh ReadRoomStaticMesh(BinaryReader reader)
    {
        return new TR3RoomStaticMesh
        {
            X = reader.ReadUInt32(),
            Y = reader.ReadUInt32(),
            Z = reader.ReadUInt32(),
            Rotation = reader.ReadUInt16(),
            Colour = reader.ReadUInt16(),
            Unused = reader.ReadUInt16(),
            MeshID = reader.ReadUInt16()
        };
    }
}
