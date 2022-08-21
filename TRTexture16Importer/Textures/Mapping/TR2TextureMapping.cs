using System.Collections.Generic;
using System.Drawing;
using System.IO;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TRTexture16Importer.Textures
{
    public class TR2TextureMapping : AbstractTextureMapping<TR2Entities, TR2Level>
    {
        protected TR2TextureMapping(TR2Level level)
            : base(level) { }

        public static TR2TextureMapping Get(TR2Level level, string mappingFilePrefix, TR2TextureDatabase database, Dictionary<StaticTextureSource<TR2Entities>, List<StaticTextureTarget>> predefinedMapping = null, List<TR2Entities> entitiesToIgnore = null)
        {
            string mapFile = Path.Combine(@"Resources\TR2\Textures\Mapping\", mappingFilePrefix + "-Textures.json");
            if (!File.Exists(mapFile))
            {
                return null;
            }

            TR2TextureMapping mapping = new TR2TextureMapping(level);
            LoadMapping(mapping, mapFile, database, predefinedMapping, entitiesToIgnore);
            return mapping;
        }

        protected override TRColour[] GetPalette8()
        {
            return _level.Palette;
        }

        protected override TRColour4[] GetPalette16()
        {
            return _level.Palette16;
        }

        protected override int ImportColour(Color colour)
        {
            return PaletteUtilities.Import(_level, colour);
        }

        protected override TRMesh[] GetModelMeshes(TR2Entities entity)
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
}