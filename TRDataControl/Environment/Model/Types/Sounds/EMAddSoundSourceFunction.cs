using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMAddSoundSourceFunction : BaseEMFunction
{
    public uint ID { get; set; }
    public TRSoundMode Mode { get; set; }
    public int X {  get; set; }
    public int Y { get; set; }
    public int Z { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        AddSource(level.SoundSources);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        AddSource(level.SoundSources);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        AddSource(level.SoundSources);
    }

    private void AddSource<T>(List<TRSoundSource<T>> sources)
        where T : Enum
    {
        sources.Add(new()
        {
            ID = (T)(object)ID,
            Mode = Mode,
            X = X,
            Y = Y,
            Z = Z,
        });
    }
}
