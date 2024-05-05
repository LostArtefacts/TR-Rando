using System.Drawing;
using TRLevelControl.Model;

namespace TRImageControl.Textures;

public class TR2TextureMapping : AbstractTextureMapping<TR2Type, TR2Level>
{
    private TRPalette16Control _paletteTracker;

    protected TR2TextureMapping(TR2Level level)
        : base(level) { }

    public static TR2TextureMapping Get(TR2Level level, string mappingFilePrefix, TR2TextureDatabase database, Dictionary<StaticTextureSource<TR2Type>, List<StaticTextureTarget>> predefinedMapping = null, List<TR2Type> entitiesToIgnore = null)
    {
        string mapFile = Path.Combine(@"Resources\TR2\Textures\Mapping\", mappingFilePrefix + "-Textures.json");
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
}
