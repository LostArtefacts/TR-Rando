namespace TRLevelControl.Model;

public class TR2SoundEffect : TRSoundEffect<TR2SFXMode>
{
    public ushort Volume { get; set; }
    public ushort Chance { get; set; }
    public uint SampleID { get; set; }
    public int SampleCount { get; set; }

    protected override void SetSampleCount(int count)
        => SampleCount = count;

    protected override int GetSampleCount()
        => SampleCount;
}
