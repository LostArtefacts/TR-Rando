using TRLevelControl.Model;

namespace TRModelTransporter.Model.Definitions;

public class TR2Blob : TRBlobBase<TR2Type>
{
    public TRCinematicFrame[] CinematicFrames { get; set; }
    public Dictionary<int, TRColour4> Colours { get; set; }
    public List<TRMesh> Meshes { get; set; }
    public TRModel Model { get; set; }
    public SortedDictionary<TR2SFX, TR2SoundEffect> SoundEffects { get; set; }

    public override bool Equals(object obj)
    {
        return obj is TR2Blob definition && Entity == definition.Entity;
    }

    public override int GetHashCode()
    {
        return 1875520522 + Entity.GetHashCode();
    }
}
