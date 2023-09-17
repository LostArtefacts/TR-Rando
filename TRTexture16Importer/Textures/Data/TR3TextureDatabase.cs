using TRLevelControl.Model;

namespace TRTexture16Importer.Textures;

public class TR3TextureDatabase : TextureDatabase<TR3Type>
{
    public TR3TextureDatabase()
        : base(@"Resources\TR3\Textures\Source") { }
}
