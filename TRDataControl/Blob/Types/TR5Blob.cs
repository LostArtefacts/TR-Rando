using TRLevelControl.Model;

namespace TRDataControl;

public class TR5Blob : TRBlobBase<TR5Type>
{
    public SortedDictionary<TR5SFX, TR4SoundEffect> SoundEffects { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR5Blob blob && ID == blob.ID;
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }
}
