using TRLevelControl.Model;

namespace TRLevelControl;

internal static class TR1FileReadUtilities
{
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
