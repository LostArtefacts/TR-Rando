using Newtonsoft.Json;
using TRDataControl;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRModelTransporter.Utilities;

public class TR2LevelTextureDeduplicator : AbstractTRLevelTextureDeduplicator<TR2Type, TR2Level>
{
    protected override TRTexturePacker CreatePacker(TR2Level level)
        => new TR2TexturePacker(level);

    protected override TRTextureRemapper<TR2Level> CreateRemapper()
        => new TR2TextureRemapper();

    protected override AbstractTextureRemapGroup<TR2Type, TR2Level> GetRemapGroup(string path)
        => JsonConvert.DeserializeObject<TR2TextureRemapGroup>(File.ReadAllText(path));
}
