﻿using System.Drawing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRTexture16Importer.Helpers;

namespace TRTexture16Importer.Textures;

public class TR3TextureMapping : AbstractTextureMapping<TR3Type, TR3Level>
{
    private TRPalette16Control _paletteTracker;

    protected TR3TextureMapping(TR3Level level)
        : base(level) { }

    public static TR3TextureMapping Get(TR3Level level, string mappingFilePrefix, TR3TextureDatabase database, Dictionary<StaticTextureSource<TR3Type>, List<StaticTextureTarget>> predefinedMapping = null, List<TR3Type> entitiesToIgnore = null, Dictionary<TR3Type, TR3Type> entityMap = null)
    {
        string mapFile = Path.Combine(@"Resources\TR3\Textures\Mapping\", mappingFilePrefix + "-Textures.json");
        if (!File.Exists(mapFile))
        {
            return null;
        }

        TR3TextureMapping mapping = new(level);
        LoadMapping(mapping, mapFile, database, predefinedMapping, entitiesToIgnore);
        mapping.EntityMap = entityMap;
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

    protected override TRMesh[] GetModelMeshes(TR3Type entity)
    {
        return TRMeshUtilities.GetModelMeshes(_level, entity);
    }

    protected override TRSpriteSequence[] GetSpriteSequences()
    {
        return _level.SpriteSequences;
    }

    protected override TRSpriteTexture[] GetSpriteTextures()
    {
        return _level.SpriteTextures;
    }

    protected override Bitmap GetTile(int tileIndex)
    {
        return _level.Images16[tileIndex].ToBitmap();
    }

    protected override void SetTile(int tileIndex, Bitmap bitmap)
    {
        _level.Images16[tileIndex].Pixels = TextureUtilities.ImportFromBitmap(bitmap);
    }
}
