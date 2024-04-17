namespace TRLevelControl.Model;

public class TR1SoundEffect : TRSoundEffect<TR1SFXMode>
{
    public ushort Volume { get; set; }
    public ushort Chance { get; set; }
    public List<byte[]> Samples { get; set; }

    protected override void SetSampleCount(int count)
        => Samples.Capacity = count;

    protected override int GetSampleCount()
        => Samples.Count;
}
