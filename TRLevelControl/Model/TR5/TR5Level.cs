namespace TRLevelControl.Model;

public class TR5Level : TRLevelBase
{
    public ushort NumRoomTextiles { get; set; }

    public ushort NumObjTextiles { get; set; }

    public ushort NumBumpTextiles { get; set; }

    public TR4Texture32Chunk Texture32Chunk { get; set; }

    public TR4Texture16Chunk Texture16Chunk { get; set; }

    public TR4SkyAndFont32Chunk SkyAndFont32Chunk { get; set; }

    public ushort LaraType { get; set; }

    public ushort WeatherType { get; set; }

    public byte[] Padding { get; set; }

    public TR5LevelDataChunk LevelDataChunk { get; set; }

    public uint NumSamples { get; set; }

    public TR4Sample[] Samples { get; set; }
}
