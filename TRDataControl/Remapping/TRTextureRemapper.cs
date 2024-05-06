using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl.Remapping;

public abstract class TRTextureRemapper<L>
    where L : TRLevelBase
{
    protected L _level;

    protected abstract TRTexturePacker CreatePacker();
    public abstract List<TRAnimatedTexture> AnimatedTextures { get; }
    public abstract List<TRObjectTexture> ObjectTextures { get; }
    public abstract IEnumerable<TRFace> Faces { get; }

    public void Remap(L level)
    {
        _level = level;
        TRTexturePacker packer = CreatePacker();

        Dictionary<TRTextileRegion, int> regionToTileMap = new();
        void CacheRegion(TRTextileRegion region, int index)
        {
            region.GenerateID();
            regionToTileMap[region] = index;
        }

        foreach (TRTextile tile in packer.Tiles)
        {
            foreach (TRTextileRegion region in tile.Rectangles)
            {
                CacheRegion(region, tile.Index);
            }
        }

        // Group each identical region
        IEnumerable<List<TRTextileRegion>> groupedRegions = regionToTileMap
            .Select(t => t.Key)
            .GroupBy(t => t.ID)
            .Where(r => r.Count() > 1 && r.All(t => t.Segments[0].Texture is TRObjectTexture))
            .Select(g => g.ToList());

        // Move identical regions to the parent in each case
        foreach (List<TRTextileRegion> regions in groupedRegions)
        {
            TRTextileRegion first = regions.First();
            for (int i = 1; i < regions.Count; i++)
            {
                TRTextileRegion region = regions[i];
                packer.Tiles[regionToTileMap[region]].Remove(region);
                region.MoveTo(first.Bounds.Location, regionToTileMap[first]);
            }
        }

        // Update the textiles
        packer.AllowEmptyPacking = true;
        packer.Pack(true);

        static string Hash(TRObjectTexture t)
        {
            return $"A{t.Atlas}B{t.Bounds}M{t.BlendingMode}T{t.HasTriangleVertex}U{t.UVMode}";
        }

        // Group each object texture
        IEnumerable<List<TRObjectTexture>> groupedTextures = ObjectTextures.GroupBy(t => Hash(t))
            .Where(g => g.Count() > 1)
            .Select(g => g.ToList());

        // Map identical object textures to the root in each case
        Dictionary<int, int> remap = new();
        foreach (List<TRObjectTexture> copies in groupedTextures)
        {
            int rootIndex = ObjectTextures.IndexOf(copies[0]);
            for (int i = 1; i < copies.Count; i++)
            {
                int j = ObjectTextures.IndexOf(copies[i]);
                remap[j] = rootIndex;
                ObjectTextures[j] = null;
            }
        }

        // Update all faces
        RemapTextures(remap);

        // Find every unused object texture and null it
        List<int> allIndices = Enumerable.Range(0, ObjectTextures.Count).ToList();
        List<int> usedIndices = Faces.Select(f => (int)f.Texture).Distinct().ToList();
        foreach (int unused in allIndices.Except(usedIndices))
        {
            ObjectTextures[unused] = null;
        }

        // Remap all indices again after deleting the nulls
        remap.Clear();
        List<TRObjectTexture> oldTextures = new(ObjectTextures);
        ObjectTextures.RemoveAll(o => o == null);
        for (int i = 0; i < oldTextures.Count; i++)
        {
            if (oldTextures[i] != null)
            {
                remap[i] = ObjectTextures.IndexOf(oldTextures[i]);
            }
        }

        // Final face update
        RemapTextures(remap);
    }

    private void RemapTextures(Dictionary<int, int> remap)
    {
        foreach (TRFace face in Faces)
        {
            if (remap.ContainsKey(face.Texture))
            {
                face.Texture = (ushort)remap[face.Texture];
            }
        }

        foreach (TRAnimatedTexture t in AnimatedTextures)
        {
            for (int i = 0; i < t.Textures.Count; i++)
            {
                if (remap.ContainsKey(t.Textures[i]))
                {
                    t.Textures[i] = (ushort)remap[t.Textures[i]];
                }
            }
        }
    }
}
