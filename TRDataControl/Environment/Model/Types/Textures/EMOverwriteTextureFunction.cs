using System.Drawing;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl.Environment;

public class EMOverwriteTextureFunction : BaseEMFunction, ITextureModifier
{
    public List<TextureOverwrite> Overwrites { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        TR1TexturePacker packer = new(level);
        ApplyOverwrites(texture =>
        {
            return packer.GetObjectTextureSegments(new List<int> { texture })
                .Select(k => new Tuple<TexturedTile, TexturedTileSegment>(k.Key, k.Value[0]))
                .First();
        });
        packer.AllowEmptyPacking = true;
        packer.Pack(true);
    }

    public override void ApplyToLevel(TR2Level level)
    {
        TR2TexturePacker packer = new(level);
        ApplyOverwrites(texture =>
        {
            return packer.GetObjectTextureSegments(new List<int> { texture })
                .Select(k => new Tuple<TexturedTile, TexturedTileSegment>(k.Key, k.Value[0]))
                .First();
        });
        packer.AllowEmptyPacking = true;
        packer.Pack(true);
    }

    public override void ApplyToLevel(TR3Level level)
    {
        TR3TexturePacker packer = new(level);
        ApplyOverwrites(texture =>
        {
            return packer.GetObjectTextureSegments(new List<int> { texture })
                .Select(k => new Tuple<TexturedTile, TexturedTileSegment>(k.Key, k.Value[0]))
                .First();
        });
        packer.AllowEmptyPacking = true;
        packer.Pack(true);
    }

    public void RemapTextures(Dictionary<ushort, ushort> indexMap)
    {
        foreach (TextureOverwrite overwrite in Overwrites)
        {
            if (indexMap.ContainsKey(overwrite.Texture))
            {
                overwrite.Texture = indexMap[overwrite.Texture];
            }
            foreach (ushort target in overwrite.Targets.Keys.ToList())
            {
                if (indexMap.ContainsKey(target))
                {
                    List<Point> points = overwrite.Targets[target];
                    overwrite.Targets.Remove(target);
                    overwrite.Targets[indexMap[target]] = points;
                }
            }
        }
    }

    private void ApplyOverwrites(Func<ushort, Tuple<TexturedTile, TexturedTileSegment>> segmentAction)
    {
        foreach (TextureOverwrite overwrite in Overwrites)
        {
            Tuple<TexturedTile, TexturedTileSegment> segment = segmentAction(overwrite.Texture);
            TRImage clippedImage = segment.Item2.Image.Export(overwrite.Clip);

            foreach (ushort targetTexture in overwrite.Targets.Keys)
            {
                Tuple<TexturedTile, TexturedTileSegment> targetSegment = segmentAction(targetTexture);
                foreach (Point point in overwrite.Targets[targetTexture])
                {
                    targetSegment.Item1.Image.Import(clippedImage, new
                    (
                        targetSegment.Item2.Bounds.X + point.X, 
                        targetSegment.Item2.Bounds.Y + point.Y
                    ), overwrite.RetainBackground);
                }
            }
        }
    }
}

public class TextureOverwrite
{
    public ushort Texture { get; set; }
    public Rectangle Clip { get; set; }
    public bool RetainBackground { get; set; }
    public Dictionary<ushort, List<Point>> Targets { get; set; }
}
