using System.Drawing;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl.Environment;

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

    private List<EMTextureMap> BuildAndPackTextures<E, L>(TRTexturePacker<E, L> packer, List<TRObjectTexture> textures)
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
            TRImage tile = packer.Tiles[indexedTexture.Atlas].BitmapGraphics;
            TRImage clip = new(tile.Extract(indexedTexture.Bounds));
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
                texture.Texture.BlendingMode = TRBlendingMode.Unused01;
            }
        }

        packer.Pack(true);

        return mappings;
    }

    private static IndexedTRObjectTexture CreateTexture(Size size)
    {
        return new()
        {
            Texture = new(0, 0, size.Width, size.Height)
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
