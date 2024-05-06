using TRLevelControl.Model;

namespace TRDataControl;

public class TR4Blob : TRBlobBase<TR4Type>
{
    public SortedDictionary<TR4SFX, TR4SoundEffect> SoundEffects { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR4Blob blob && ID == blob.ID;
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }
}
