namespace TRLevelControl.Model;

public class TR3SoundEffect : TRSoundEffect<TR3SFXMode>
{
    public byte Volume { get; set; }
    public byte Chance { get; set; }
    public byte Range { get; set; }
    public byte Pitch { get; set; }
    public uint SampleID { get; set; }
    public int SampleCount { get; set; }

    protected override void SetSampleCount(int count)
        => SampleCount = count;

    protected override int GetSampleCount()
        => SampleCount;
}
