using TRLevelControl.Model;

namespace TRImageControl.Textures;

public class TR3TextureDatabase : TextureDatabase<TR3Type>
{
    public TR3TextureDatabase()
        : base(@"Resources\TR3\Textures\Source") { }
}
