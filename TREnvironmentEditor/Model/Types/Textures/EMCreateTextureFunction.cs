using System.Drawing;
using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;
using TRTexture16Importer.Helpers;

namespace TREnvironmentEditor.Model.Types;

public class EMCreateTextureFunction : BaseEMFunction
{
    public List<EMTextureData> Data { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        TR1TexturePacker packer = new(level);
        List<TRObjectTexture> textures = new(level.ObjectTextures);

        List<EMTextureMap> mapping = BuildAndPackTextures(packer, level.ObjectTextures);
        mapping.ForEach(m => new EMRefaceFunction
        {
            TextureMap = m
        }.ApplyToLevel(level));
    }

    public override void ApplyToLevel(TR2Level level)
    {
        TR2TexturePacker packer = new(level);
        List<TRObjectTexture> textures = new(level.ObjectTextures);

        List<EMTextureMap> mapping = BuildAndPackTextures(packer, level.ObjectTextures);
        mapping.ForEach(m => new EMRefaceFunction
        {
            TextureMap = m
        }.ApplyToLevel(level));
    }

    public override void ApplyToLevel(TR3Level level)
    {
        TR3TexturePacker packer = new(level);
        List<TRObjectTexture> textures = new(level.ObjectTextures);

        List<EMTextureMap> mapping = BuildAndPackTextures(packer, level.ObjectTextures);
        mapping.ForEach(m => new EMRefaceFunction
        {
            TextureMap = m
        }.ApplyToLevel(level));
    }

    private List<EMTextureMap> BuildAndPackTextures<E, L>(AbstractTexturePacker<E, L> packer, List<TRObjectTexture> textures)
        where L : class
        where E : Enum
    {
        List<EMTextureMap> mappings = new();

        foreach (EMTextureData data in Data)
        {
            IndexedTRObjectTexture indexedTexture = new()
            {
                Index = data.Background,
                Texture = textures[data.Background]
            };
            BitmapGraphics tile = packer.Tiles[indexedTexture.Atlas].BitmapGraphics;
            BitmapGraphics clip = new(tile.Extract(indexedTexture.Bounds));
            clip.Overlay(new Bitmap(data.Overlay));

            IndexedTRObjectTexture texture = CreateTexture(clip.Bitmap.Size);
            TexturedTileSegment segment = new(texture, clip.Bitmap);
            packer.AddRectangle(segment);

            mappings.Add(new()
            {
                [(ushort)textures.Count] = data.GeometryMap
            });
            textures.Add(texture.Texture);

            // Use a flag that's unused throughout the games to indicate this texture
            // must remain as-is.
            if (data.RetainInWireframe)
            {
                texture.Texture.Attribute = (ushort)TRBlendingMode.Unused01;
            }
        }

        packer.Pack(true);

        return mappings;
    }

    private static IndexedTRObjectTexture CreateTexture(Size size)
    {
        return new()
        {
            Texture = new()
            {
                Vertices = new TRObjectTextureVert[]
                {
                    CreatePoint(0, 0),
                    CreatePoint(size.Width, 0),
                    CreatePoint(size.Width, size.Height),
                    CreatePoint(0, size.Height)
                }
            }
        };
    }

    private static TRObjectTextureVert CreatePoint(int x, int y)
    {
        return new()
        {
            XCoordinate = new()
            {
                Whole = (byte)(x == 0 ? 1 : 255),
                Fraction = (byte)(x == 0 ? 0 : x - 1)
            },
            YCoordinate = new()
            {
                Whole = (byte)(y == 0 ? 1 : 255),
                Fraction = (byte)(y == 0 ? 0 : y - 1)
            }
        };
    }
}

public class EMTextureData
{
    public ushort Background { get; set; }
    public string Overlay { get; set; }
    public bool RetainInWireframe { get; set; }
    public EMGeometryMap GeometryMap { get; set; }
}
