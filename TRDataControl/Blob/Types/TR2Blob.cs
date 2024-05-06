using TRLevelControl.Model;

namespace TRDataControl;

public class TR2Blob : TRBlobBase<TR2Type>
{
    public Dictionary<ushort, TRColour4> Palette16 { get; set; }
    public SortedDictionary<TR2SFX, TR2SoundEffect> SoundEffects { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR2Blob blob && ID == blob.ID;
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }
}
