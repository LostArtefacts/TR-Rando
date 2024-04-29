using System.Diagnostics;
using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR3FileReadUtilities
{
    public static TR3RoomData ConvertToRoomData(ushort[] rawData)
    {
        // This approach is temporarily retained

        TR3RoomData roomData = new()
        {
            Vertices = new()
        };

        int offset = 0;
        ushort count = rawData[offset++];
        for (int j = 0; j < count; j++)
        {
            roomData.Vertices.Add(new()
            {
                Vertex = new()
                {
                    X = UnsafeConversions.UShortToShort(rawData[offset++]),
                    Y = UnsafeConversions.UShortToShort(rawData[offset++]),
                    Z = UnsafeConversions.UShortToShort(rawData[offset++]),
                },
                Lighting = UnsafeConversions.UShortToShort(rawData[offset++]),
                Attributes = rawData[offset++],
                Colour = rawData[offset++],
            });
        }

        count = rawData[offset++];
        roomData.Rectangles = new();
        for (int j = 0; j < count; j++)
        {
            roomData.Rectangles.Add(new()
            {
                Vertices = new ushort[]
                {
                    rawData[offset++],
                    rawData[offset++],
                    rawData[offset++],
                    rawData[offset++],
                },
                Texture = rawData[offset++],
            });
        }

        count = rawData[offset++];
        roomData.Triangles = new();
        for (int j = 0; j < count; j++)
        {
            roomData.Triangles.Add(new()
            {
                Vertices = new ushort[]
                {
                    rawData[offset++],
                    rawData[offset++],
                    rawData[offset++],
                },
                Texture = rawData[offset++],
            });
        }

        count = rawData[offset++];
        roomData.Sprites = new();
        for (int j = 0; j < count; j++)
        {
            roomData.Sprites.Add(new()
            {
                Vertex = UnsafeConversions.UShortToShort(rawData[offset++]),
                Texture = UnsafeConversions.UShortToShort(rawData[offset++]),
            });
        }

        Debug.Assert(offset == rawData.Length);

        return roomData;
    }

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
