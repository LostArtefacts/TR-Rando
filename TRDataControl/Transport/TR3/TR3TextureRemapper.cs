using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR3TextureRemapper : TRTextureRemapper<TR3Level>
{
    public override IEnumerable<TRFace> RoomFaces
        => _level.Rooms.Select(r => r.Mesh).SelectMany(m => m.Faces);

    protected override TRTexturePacker CreatePacker()
        => new TR3TexturePacker(_level, 32);
}
