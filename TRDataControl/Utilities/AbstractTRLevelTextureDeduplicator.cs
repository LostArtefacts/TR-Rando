using TRDataControl;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRModelTransporter.Utilities;

public abstract class AbstractTRLevelTextureDeduplicator<T, L>
    where T : Enum
    where L : TRLevelBase
{
    public L Level { get; set; }

    private readonly TRTextureDeduplicator<T> _deduplicator;

    public AbstractTRLevelTextureDeduplicator()
    {
        _deduplicator = new()
        {
            UpdateGraphics = true
        };
    }

    public void Deduplicate(string remappingPath)
    {
        TRTexturePacker levelPacker = CreatePacker(Level);
        Dictionary<TRTextile, List<TRTextileRegion>> allTextures = new();
        foreach (TRTextile tile in levelPacker.Tiles)
        {
            allTextures[tile] = new List<TRTextileRegion>(tile.Rectangles);
        }

        AbstractTextureRemapGroup<T, L> remapGroup = GetRemapGroup(remappingPath);

        _deduplicator.SegmentMap = allTextures;
        _deduplicator.PrecompiledRemapping = remapGroup.Remapping;
        _deduplicator.Deduplicate();

        levelPacker.AllowEmptyPacking = true;
        levelPacker.Pack(true);

        CreateRemapper(Level).Remap();
    }

    protected abstract TRTexturePacker CreatePacker(L level);
    protected abstract TRTextureRemapper<L> CreateRemapper(L level);
    protected abstract AbstractTextureRemapGroup<T, L> GetRemapGroup(string path);
}
