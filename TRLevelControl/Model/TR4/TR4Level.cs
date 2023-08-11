namespace TRLevelControl.Model;

public class TR4Level : TRLevelBase
{
    public TR4Texture32Chunk Texture32Chunk { get; set; }

    public TR4Texture16Chunk Texture16Chunk { get; set; }

    public TR4SkyAndFont32Chunk SkyAndFont32Chunk { get; set; }

    public TR4LevelDataChunk LevelDataChunk { get; set; }

    public uint NumSamples { get; set; }

    public TR4Sample[] Samples { get; set; }
}
