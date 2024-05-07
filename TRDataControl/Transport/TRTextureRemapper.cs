using TRImageControl.Packing;
using TRLevelControl.Model;

namespace TRDataControl;

public abstract class TRTextureRemapper<L>
    where L : TRLevelBase
{
    protected L _level;

    protected abstract TRTexturePacker CreatePacker();
    public abstract IEnumerable<TRFace> RoomFaces { get; }

    public IEnumerable<TRFace> AllFaces
        => RoomFaces.Concat(_level.DistinctMeshes.SelectMany(m => m.TexturedFaces));

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
        IEnumerable<List<TRObjectTexture>> groupedTextures = level.ObjectTextures.GroupBy(t => Hash(t))
            .Where(g => g.Count() > 1)
            .Select(g => g.ToList());

        // Map identical object textures to the root in each case
        Dictionary<int, int> remap = new();
        foreach (List<TRObjectTexture> copies in groupedTextures)
        {
            int rootIndex = level.ObjectTextures.IndexOf(copies[0]);
            for (int i = 1; i < copies.Count; i++)
            {
                int j = level.ObjectTextures.IndexOf(copies[i]);
                remap[j] = rootIndex;
                level.ObjectTextures[j].Atlas = ushort.MaxValue;
            }
        }

        // Update all faces
        RemapTextures(level, remap);

        ResetUnusedTextures(_level);
    }

    public void ResetUnusedTextures(L level)
    {
        _level = level;

        // Find every unused object texture and null it
        IEnumerable<int> allIndices = Enumerable.Range(0, level.ObjectTextures.Count);
        IEnumerable<int> usedIndices = AllFaces.Select(f => (int)f.Texture)
            .Concat(level.AnimatedTextures.SelectMany(a => a.Textures.Select(t => (int)t)))
            .Distinct();
        foreach (int unused in allIndices.Except(usedIndices))
        {
            level.ObjectTextures[unused].Atlas = ushort.MaxValue;
        }

        // Remap all indices after deleting the nulls
        Dictionary<int, int> remap = new();
        List<TRObjectTexture> oldTextures = new(level.ObjectTextures);
        level.ObjectTextures.RemoveAll(o => o.Atlas == ushort.MaxValue);
        for (int i = 0; i < oldTextures.Count; i++)
        {
            if (oldTextures[i].Atlas != ushort.MaxValue)
            {
                remap[i] = level.ObjectTextures.IndexOf(oldTextures[i]);
            }
        }

        // Update all faces
        RemapTextures(level, remap);
    }

    private void RemapTextures(L level, Dictionary<int, int> remap)
    {
        foreach (TRFace face in AllFaces)
        {
            if (remap.ContainsKey(face.Texture))
            {
                face.Texture = (ushort)remap[face.Texture];
            }
        }

        foreach (TRAnimatedTexture t in level.AnimatedTextures)
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
