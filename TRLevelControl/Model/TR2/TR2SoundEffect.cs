namespace TRLevelControl.Model;

public class TR2SoundEffect : TRSoundEffect<TR2SFXMode>
{
    public ushort Volume { get; set; }
    public ushort Chance { get; set; }
    public List<uint> Samples { get; set; }

    protected override void SetSampleCount(int count)
        => Samples.Capacity = count;

    protected override int GetSampleCount()
        => Samples.Count;
}
