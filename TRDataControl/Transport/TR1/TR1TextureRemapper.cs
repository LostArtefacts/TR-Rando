using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR1TextureRemapper : TRTextureRemapper<TR1Level>
{
    public override IEnumerable<TRFace> RoomFaces
        => _level.Rooms.Select(r => r.Mesh).SelectMany(m => m.Faces);

    protected override TRTexturePacker CreatePacker()
        => new TR1TexturePacker(_level, 32);

    public TR1TextureRemapper(TR1Level level)
        : base(level) { }
}
