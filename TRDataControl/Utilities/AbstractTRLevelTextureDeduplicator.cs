using TRDataControl;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRModelTransporter.Utilities;

public abstract class AbstractTRLevelTextureDeduplicator<E, L>
    where E : Enum
    where L : TRLevelBase
{
    public L Level { get; set; }

    private readonly TRTextureDeduplicator<E> _deduplicator;

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

        AbstractTextureRemapGroup<E, L> remapGroup = GetRemapGroup(remappingPath);

        _deduplicator.SegmentMap = allTextures;
        _deduplicator.PrecompiledRemapping = remapGroup.Remapping;
        _deduplicator.Deduplicate();

        levelPacker.AllowEmptyPacking = true;
        levelPacker.Pack(true);

        CreateRemapper().Remap(Level);
    }

    protected abstract TRTexturePacker CreatePacker(L level);
    protected abstract TRTextureRemapper<L> CreateRemapper();
    protected abstract AbstractTextureRemapGroup<E, L> GetRemapGroup(string path);
}
