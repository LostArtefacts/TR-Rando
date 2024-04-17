namespace TRLevelControl.Model;

public class TR1SoundEffect : TRSoundEffect<TR1SFXMode>, ICloneable
{
    public ushort Volume { get; set; }
    public ushort Chance { get; set; }
    public List<byte[]> Samples { get; set; }

    protected override void SetSampleCount(int count)
        => Samples.Capacity = count;

    protected override int GetSampleCount()
        => Samples.Count;

    public TR1SoundEffect Clone()
    {
        return new()
        {
            Mode = Mode,
            Pan = Pan,
            RandomizePitch = RandomizePitch,
            RandomizeVolume = RandomizeVolume,
            Volume = Volume,
            Chance = Chance,
            Samples = new(Samples.Select(s => (byte[])s.Clone()))
        };
    }

    object ICloneable.Clone()
        => Clone();
}
