using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl.Remapping;

public class TR4TextureRemapper : TRTextureRemapper<TR4Level>
{
    public override List<TRAnimatedTexture> AnimatedTextures
        => _level.AnimatedTextures;

    public override List<TRObjectTexture> ObjectTextures
        => _level.ObjectTextures;

    public override IEnumerable<TRFace> Faces
        => _level.Rooms.Select(r => r.Mesh)
        .SelectMany(m => m.Faces)
        .Concat(_level.DistinctMeshes.SelectMany(m => m.TexturedFaces));

    protected override TRTexturePacker CreatePacker()
        => new TR4TexturePacker(_level, TRGroupPackingMode.Object, 32);
}
