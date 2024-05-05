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

    private List<EMTextureMap> BuildAndPackTextures(TRTexturePacker packer, List<TRObjectTexture> textures)
    {
        List<EMTextureMap> mappings = new();

        foreach (EMTextureData data in Data)
        {
            TRTextileSegment indexedTexture = new()
            {
                Index = data.Background,
                Texture = textures[data.Background]
            };
            TRImage tile = packer.Tiles[indexedTexture.Atlas].Image;
            TRImage clip = tile.Export(indexedTexture.Bounds);
            clip.Overlay(new(data.Overlay));

            TRTextileSegment segment = new()
            {
                Texture = new TRObjectTexture(new(0, 0, clip.Size.Width, clip.Size.Height))
                {
                    BlendingMode = data.RetainInWireframe ? TRBlendingMode.Unused01 : TRBlendingMode.Opaque,
                }
            };

            packer.AddRectangle(new(segment, clip));

            mappings.Add(new()
            {
                [(ushort)textures.Count] = data.GeometryMap
            });
            textures.Add(segment.Texture as TRObjectTexture);
        }

        packer.Pack(true);

        return mappings;
    }
}

public class EMTextureData
{
    public ushort Background { get; set; }
    public string Overlay { get; set; }
    public bool RetainInWireframe { get; set; }
    public EMGeometryMap GeometryMap { get; set; }
}
