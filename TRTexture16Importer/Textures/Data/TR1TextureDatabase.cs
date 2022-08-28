using TRLevelReader.Model.Enums;

namespace TRTexture16Importer.Textures
{
    public class TR1TextureDatabase : TextureDatabase<TREntities>
    {
        public TR1TextureDatabase()
            : base(@"Resources\TR1\Textures\Source") { }
    }
}