using TRLevelControl.Model;

namespace TRModelTransporter.Model.Definitions;

public class TR3Blob : TRBlobBase<TR3Type>
{
    public TRCinematicFrame[] CinematicFrames { get; set; }
    public Dictionary<int, TRColour4> Colours { get; set; }
    public List<TRMesh> Meshes { get; set; }
    public TRModel Model { get; set; }
    public SortedDictionary<TR3SFX, TR3SoundEffect> SoundEffects { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR3Blob definition && Entity == definition.Entity;
    }

    public override int GetHashCode()
    {
        return 1075520522 + Entity.GetHashCode();
    }
}
