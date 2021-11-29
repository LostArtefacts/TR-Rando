using TRLevelReader.Model.Enums;

namespace TRTexture16Importer.Textures
{
    public class TR2TextureDatabase : TextureDatabase<TR2Entities>
    {
        public TR2TextureDatabase()
            : base(@"Resources\TR2\Textures\Source") { }
    }
}