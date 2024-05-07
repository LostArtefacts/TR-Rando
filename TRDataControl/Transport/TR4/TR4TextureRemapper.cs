using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR4TextureRemapper : TRTextureRemapper<TR4Level>
{
    public override IEnumerable<TRFace> RoomFaces
        => _level.Rooms.Select(r => r.Mesh).SelectMany(m => m.Faces);

    protected override TRTexturePacker CreatePacker()
        => new TR4TexturePacker(_level, TRGroupPackingMode.Object, 32);
}
