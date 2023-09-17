using TRLevelControl.Model;

namespace TRTexture16Importer.Textures;

public class TR2TextureDatabase : TextureDatabase<TR2Type>
{
    public TR2TextureDatabase()
        : base(@"Resources\TR2\Textures\Source") { }
}
