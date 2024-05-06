using Newtonsoft.Json;
using System.Drawing;
using TRImageControl;
using TRLevelControl.Model;

namespace TRModelTransporter.Model;

public abstract class TRBlobBase<E> where E : Enum
{
    [JsonIgnore]
    public E Entity { get; set; }
    [JsonIgnore]
    public E Alias { get; set; }
    [JsonIgnore]
    public TRImage Image { get; set; }
    [JsonIgnore]
    public bool HasGraphics => false;// ObjectTextures.Count > 0;
    [JsonIgnore]
    public bool IsDependencyOnly { get; set; }

    public E[] Dependencies { get; set; }        
    //public Dictionary<int, List<IndexedTRObjectTexture>> ObjectTextures { get; set; }
    //public Dictionary<E, Dictionary<int, List<IndexedTRSpriteTexture>>> SpriteTextures { get; set; }
    public Dictionary<E, TRSpriteSequence> SpriteSequences { get; set; }
    public Rectangle[] TextureSegments { get; set; }
}
