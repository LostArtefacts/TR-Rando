using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public class TR2TextureRemapper : TRTextureRemapper<TR2Level>
{
    public override IEnumerable<TRFace> RoomFaces
        => _level.Rooms.Select(r => r.Mesh).SelectMany(m => m.Faces);

    protected override TRTexturePacker CreatePacker()
        => new TR2TexturePacker(_level, 32);
}
