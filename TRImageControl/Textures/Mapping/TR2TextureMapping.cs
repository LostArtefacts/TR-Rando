using System.Drawing;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRImageControl.Textures;

public class TR2TextureMapping : AbstractTextureMapping<TR2Type, TR2Level>
{
    private TRPalette16Control _paletteTracker;

    protected TR2TextureMapping(TR2Level level)
        : base(level) { }

    public static TR2TextureMapping Get(TR2Level level, string mappingFilePrefix, TR2TextureDatabase database, Dictionary<StaticTextureSource<TR2Type>, List<StaticTextureTarget>> predefinedMapping = null, List<TR2Type> entitiesToIgnore = null)
    {
        string mapFile = Path.Combine("Resources/TR2/Textures/Mapping/", mappingFilePrefix + "-Textures.json");
        if (!File.Exists(mapFile))
        {
            return null;
        }

        TR2TextureMapping mapping = new(level);
        LoadMapping(mapping, mapFile, database, predefinedMapping, entitiesToIgnore);
        return mapping;
    }

    protected override List<TRColour> GetPalette8()
    {
        return _level.Palette;
    }

    protected override List<TRColour4> GetPalette16()
    {
        return _level.Palette16;
    }

    protected override int ImportColour(Color colour)
    {
        _paletteTracker ??= new(_level);
        return _paletteTracker.Import(colour);
    }

    protected override List<TRMesh> GetModelMeshes(TR2Type entity)
    {
        return _level.Models.ContainsKey(entity) ? _level.Models[entity].Meshes : null;
    }

    protected override TRDictionary<TR2Type, TRSpriteSequence> GetSpriteSequences()
    {
        return _level.Sprites;
    }

    protected override TRImage GetTile(int tileIndex)
    {
        return new(_level.Images16[tileIndex].Pixels);
    }

    protected override void SetTile(int tileIndex, TRImage image)
    {
        _level.Images16[tileIndex].Pixels = image.ToRGB555();
    }

    protected override void GenerateSpriteTargets(StaticTextureSource<TR2Type> source, string variant)
    {
        // Temporary workaround until TR2 uses the same approach as TR1 for dynamic mapping.
        // Only applies to font/UI frame.
        var sprites = GetSpriteSequences();
        if (!sprites.TryGetValue(source.SpriteSequence, out var sequence)
            || source.DynamicMap == null || !source.DynamicMap.TryGetValue(variant, out var hsb))
        {
            return;
        }

        var target = new Dictionary<int, List<Rectangle>>();
        var packer = new TR2TexturePacker(_level);

        var regions = packer.GetSpriteRegions(sequence);
        foreach (var tile in regions.Keys)
        {
            if (!target.TryGetValue(tile.Index, out var rects))
            {
                target[tile.Index] = rects = [];
            }
            rects.AddRange(regions[tile].Select(s => s.Bounds));
        }
        
        RedrawDynamicTargets(target, hsb);
    }
}
