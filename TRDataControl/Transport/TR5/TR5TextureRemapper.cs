using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR5TextureRemapper : TRTextureRemapper<TR5Level>
{
    public override IEnumerable<TRFace> RoomFaces
        => _level.Rooms.Select(r => r.Mesh).SelectMany(m => m.Faces);

    protected override TRTexturePacker CreatePacker()
        => new TR5TexturePacker(_level, TRGroupPackingMode.Object, 32);
}
