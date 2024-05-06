using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public abstract class TRBlobBase<T>
    where T : Enum
{
    public TRBlobType Type { get; set; }
    public T ID { get; set; }
    public T Alias { get; set; }
    public bool IsDependencyOnly { get; set; }
    public List<T> Dependencies { get; set; }
    public TRModel Model { get; set; }
    public TRStaticMesh StaticMesh { get; set; }
    public List<int> SpriteOffsets { get; set; }
    public List<TRTextileRegion> Textures { get; set; }
    public List<TRCinematicFrame> CinematicFrames { get; set; }
}
