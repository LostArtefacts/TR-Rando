using TRDataControl;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Utilities;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Processors.TR2.Tasks;

public class TR2XDeduplicationTask : ITR2ProcessorTask
{
    public void Run(TR2CombinedLevel level)
    {
        if (level.Is(TR2LevelNames.FLOATER) && level.Data.AnimatedTextures[0].Textures[0] == 1702)
        {
            level.IsUKBox = true;
        }

        if (level.Is(TR2LevelNames.FOOLGOLD))
        {
            level.Data.Sprites[TR2Type.FireBlast_S_H] = level.Data.Sprites[TR2Type.Explosion_S_H].Clone();
        }

        var dedupPath = $"Resources/TR2/Textures/Deduplication/{level.JsonID}-TextureRemap.json";
        if (!File.Exists(dedupPath))
        {
            return;
        }

        var levelPacker = new TR2TexturePacker(level.Data);
        var allTextures = new Dictionary<TRTextile, List<TRTextileRegion>>();
        foreach (var tile in levelPacker.Tiles)
        {
            allTextures[tile] = [.. tile.Rectangles];
        }

        var remapGroup = JsonUtils.ReadFile<TR2TextureRemapGroup>(dedupPath);
        var deduplicator = new TRTextureDeduplicator<TR2Type>
        {
            UpdateGraphics = true,
            SegmentMap = allTextures,
            PrecompiledRemapping = remapGroup.Remapping,
        };

        deduplicator.Deduplicate();

        levelPacker.AllowEmptyPacking = true;
        levelPacker.Pack(true);

        var remapper = new TR2TextureRemapper(level.Data);
        remapper.ResetUnusedTextures();
    }
}
