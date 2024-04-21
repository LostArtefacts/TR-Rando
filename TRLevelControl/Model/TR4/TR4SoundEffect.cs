namespace TRLevelControl.Model;

public class TR4SoundEffect : TRSoundEffect<TR3SFXMode>
{
    public byte Volume { get; set; }
    public byte Chance { get; set; }
    public byte Range { get; set; }
    public byte Pitch { get; set; }
    public List<TR4Sample> Samples { get; set; }

    protected override void SetSampleCount(int count)
        => Samples.Capacity = count;

    protected override int GetSampleCount()
        => Samples.Count;
}
