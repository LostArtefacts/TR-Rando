using TRImageControl.Packing;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRModelTransporter.Utilities;

public class MassTR2TextureDeduplicator : AbstractMassTRTextureDeduplicator<TR2Type, TR2Level>
{
    public override List<string> LevelNames => TR2LevelNames.AsList;

    private readonly TR2LevelControl _control;

    public MassTR2TextureDeduplicator()
    {
        _control = new();
    }

    protected override TRTexturePacker CreatePacker(TR2Level level)
    {
        return new TR2TexturePacker(level);
    }

    protected override AbstractTextureRemapGroup<TR2Type, TR2Level> CreateRemapGroup()
    {
        return new TR2TextureRemapGroup();
    }

    protected override AbstractTRLevelTextureDeduplicator<TR2Type, TR2Level> CreateDeduplicator()
    {
        return new TR2LevelTextureDeduplicator();
    }

    protected override TR2Level ReadLevel(string path)
    {
        return _control.Read(path);
    }

    protected override void WriteLevel(TR2Level level, string path)
    {
        _control.Write(level, path);
    }
}
