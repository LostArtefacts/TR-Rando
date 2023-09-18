using TRLevelControl.Model;

namespace TRTexture16Importer.Textures;

public class TR1TextureDatabase : TextureDatabase<TR1Type>
{
    public TR1TextureDatabase()
        : base(@"Resources\TR1\Textures\Source") { }
}
