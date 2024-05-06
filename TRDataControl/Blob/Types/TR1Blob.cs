using TRLevelControl.Model;

namespace TRModelTransporter.Model.Definitions;

public class TR1Blob : TRBlobBase<TR1Type>
{
    public TRCinematicFrame[] CinematicFrames { get; set; }
    public Dictionary<int, TRColour> Colours { get; set; }
    public List<TRMesh> Meshes { get; set; }
    public TRModel Model { get; set; }
    public SortedDictionary<TR1SFX, TR1SoundEffect> SoundEffects { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR1Blob definition && Entity == definition.Entity;
    }

    public override int GetHashCode()
    {
        return 1674515507 + Entity.GetHashCode();
    }
}
