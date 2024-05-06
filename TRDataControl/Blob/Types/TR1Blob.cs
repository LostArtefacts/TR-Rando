using TRLevelControl.Model;

namespace TRDataControl;

public class TR1Blob : TRBlobBase<TR1Type>
{
    public Dictionary<ushort, TRColour> Palette8 { get; set; }
    public SortedDictionary<TR1SFX, TR1SoundEffect> SoundEffects { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR1Blob blob && ID == blob.ID;
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }
}
