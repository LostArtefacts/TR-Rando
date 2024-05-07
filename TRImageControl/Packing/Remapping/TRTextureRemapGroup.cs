using System.Drawing;
using TRLevelControl.Model;

namespace TRImageControl.Packing;

public abstract class TRTextureRemapGroup<T, L> 
    where T : Enum
    where L : TRLevelBase
{
    public List<TextureRemap> Remapping { get; set; } = new();
    public List<TextureDependency<T>> Dependencies { get; set; } = new();

    public void CalculateDependencies(L level)
    {
        TRTexturePacker packer = CreatePacker(level);
        TRDictionary<T, TRModel> models = GetModels(level);
        TRMesh dummyMesh = GetDummyMesh(level);

        foreach (var (baseType, baseModel) in models)
        {
            Dictionary<TRTextile, List<TRTextileRegion>> baseRegions = packer.GetMeshRegions(baseModel.Meshes,
                IsMasterType(baseType) ? dummyMesh : null);

            foreach (var (otherType, otherModel) in models)
            {
                if (EqualityComparer<T>.Default.Equals(baseType, otherType))
                {
                    continue;
                }

                Dictionary<TRTextile, List<TRTextileRegion>> otherRegions = packer.GetMeshRegions(otherModel.Meshes,
                    IsMasterType(otherType) ? dummyMesh : null);

                foreach (var (tile, regions) in baseRegions)
                {
                    if (!otherRegions.ContainsKey(tile))
                    {
                        continue;
                    }

                    List<TRTextileRegion> matches = regions.FindAll(s1 => otherRegions[tile].Any(s2 => s1 == s2));
                    foreach (TRTextileRegion matchedRegion in matches)
                    {
                        TextureDependency<T> dependency = GetDependency(tile.Index, matchedRegion.Bounds);
                        if (dependency == null)
                        {
                            dependency = new()
                            {
                                TileIndex = tile.Index,
                                Bounds = matchedRegion.Bounds
                            };
                            Dependencies.Add(dependency);
                        }
                        dependency.AddType(baseType);
                        dependency.AddType(otherType);
                    }
                }
            }
        }
    }

    protected abstract TRTexturePacker CreatePacker(L level);
    protected abstract TRDictionary<T, TRModel> GetModels(L level);
    protected abstract bool IsMasterType(T type);
    protected abstract TRMesh GetDummyMesh(L level);

    public TextureDependency<T> GetDependency(int tileIndex, Rectangle rectangle)
    {
        return Dependencies.Find(d => d.TileIndex == tileIndex && d.Bounds == rectangle);
    }

    public bool CanRemoveRectangle(int tileIndex, Rectangle rectangle, IEnumerable<T> types)
    {
        // The rectangle can be removed if all of the types that use it are being removed
        TextureDependency<T> dependency = GetDependency(tileIndex, rectangle);
        return dependency?.Types.All(types.Contains) ?? true;
    }
}
