using Newtonsoft.Json;
using TRDataControl;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRModelTransporter.Utilities;

public class TR2LevelTextureDeduplicator : AbstractTRLevelTextureDeduplicator<TR2Type, TR2Level>
{
    protected override TRTexturePacker CreatePacker(TR2Level level)
    {
        return new TR2TexturePacker(level);
    }

    protected override AbstractTextureRemapGroup<TR2Type, TR2Level> GetRemapGroup(string path)
    {
        return JsonConvert.DeserializeObject<TR2TextureRemapGroup>(File.ReadAllText(path));
    }

    protected override void ReindexTextures(Dictionary<int, int> indexMap)
    {
        Level.ReindexTextures(indexMap);
        Level.ResetUnusedTextures();
    }
}
