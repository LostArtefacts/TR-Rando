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
            return packer.GetObjectRegions(new List<int> { texture })
                .Select(k => new Tuple<TRTextile, TRTextileRegion>(k.Key, k.Value[0]))
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
            return packer.GetObjectRegions(new List<int> { texture })
                .Select(k => new Tuple<TRTextile, TRTextileRegion>(k.Key, k.Value[0]))
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
            return packer.GetObjectRegions(new List<int> { texture })
                .Select(k => new Tuple<TRTextile, TRTextileRegion>(k.Key, k.Value[0]))
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

    private void ApplyOverwrites(Func<ushort, Tuple<TRTextile, TRTextileRegion>> regionAction)
    {
        foreach (TextureOverwrite overwrite in Overwrites)
        {
            Tuple<TRTextile, TRTextileRegion> region = regionAction(overwrite.Texture);
            TRImage clippedImage = region.Item2.Image.Export(overwrite.Clip);

            foreach (ushort targetTexture in overwrite.Targets.Keys)
            {
                Tuple<TRTextile, TRTextileRegion> targetRegion = regionAction(targetTexture);
                foreach (Point point in overwrite.Targets[targetTexture])
                {
                    targetRegion.Item1.Image.Import(clippedImage, new
                    (
                        targetRegion.Item2.Bounds.X + point.X, 
                        targetRegion.Item2.Bounds.Y + point.Y
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
