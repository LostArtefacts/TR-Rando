using TRLevelControl.Model;

namespace TRDataControl;

public class TR3Blob : TRBlobBase<TR3Type>
{
    public Dictionary<ushort, TRColour4> Palette16 { get; set; }
    public SortedDictionary<TR3SFX, TR3SoundEffect> SoundEffects { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR3Blob blob && ID == blob.ID;
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }
}
