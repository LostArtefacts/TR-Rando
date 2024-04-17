namespace TRLevelControl.Model;

public abstract class TRSoundEffect<T>
    where T : Enum
{
    public T Mode { get; set; }
    public bool Pan { get; set; }
    public bool RandomizePitch { get; set; }
    public bool RandomizeVolume { get; set; }

    public void SetFlags(ushort flags)
    {
        Mode = (T)(object)(byte)(flags & 3);
        Pan = (flags & (1 << 12)) == 0; // Game interprets it as NoPan
        RandomizePitch = (flags & (1 << 13)) > 0;
        RandomizeVolume = (flags & (1 << 14)) > 0;

        SetSampleCount((flags & 0x00FC) >> 2);
    }

    public ushort GetFlags()
    {
        ushort flags = (byte)(object)Mode;
        if (!Pan)
        {
            flags |= (1 << 12);
        }
        if (RandomizePitch)
        {
            flags |= (1 << 13);
        }
        if (RandomizeVolume)
        {
            flags |= (1 << 14);
        }

        flags |= (ushort)((GetSampleCount() << 2) & 0x00FC);

        return flags;
    }

    protected abstract void SetSampleCount(int count);
    protected abstract int GetSampleCount();
}
