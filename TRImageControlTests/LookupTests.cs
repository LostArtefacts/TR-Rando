﻿using TRImageControl.Packing;
using TRLevelControl.Model;
using TRLevelControlTests;

namespace TRImageControlTests;

[TestClass]
public class LookupTests : TestBase
{
    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test locating mesh texture regions in TR1.")]
    public void TestGetTR1MeshRegions()
    {
        TR1Level level = GetTR1TestLevel();
        TR1TexturePacker packer = new(level, 16);

        TestGetMeshRegions(packer, level.Models[TR1Type.Lara].Meshes, level.ObjectTextures);
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test locating mesh texture regions in TR2.")]
    public void TestGetTR2MeshRegions()
    {
        TR2Level level = GetTR2TestLevel();
        TR2TexturePacker packer = new(level, 16);

        TestGetMeshRegions(packer, level.Models[TR2Type.Lara].Meshes, level.ObjectTextures);
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test locating mesh texture regions in TR3.")]
    public void TestGetTR3MeshRegions()
    {
        TR3Level level = GetTR3TestLevel();
        TR3TexturePacker packer = new(level, 16);

        TestGetMeshRegions(packer, level.Models[TR3Type.Lara].Meshes, level.ObjectTextures);
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test locating mesh texture regions in TR4.")]
    public void TestGetTR4MeshRegions()
    {
        TR4Level level = GetTR4TestLevel();
        TR4TexturePacker packer = new(level, TRGroupPackingMode.Object, 32);

        TestGetMeshRegions(packer, level.Models[TR4Type.Lara].Meshes, level.ObjectTextures);
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test locating mesh texture regions in TR5.")]
    public void TestGetTR5MeshRegions()
    {
        TR5Level level = GetTR5TestLevel();
        TR5TexturePacker packer = new(level, TRGroupPackingMode.Object, 32);

        TestGetMeshRegions(packer, level.Models[TR5Type.Lara].Meshes, level.ObjectTextures);
    }

    private static void TestGetMeshRegions(TRTexturePacker packer, List<TRMesh> meshes, List<TRObjectTexture> allTextures)
    {
        Dictionary<TRTextile, List<TRTextileRegion>> regions = packer.GetMeshRegions(meshes);

        Tuple<TRTextile, TRTextileSegment> FindSegment(ushort texture)
        {
            foreach (TRTextile tile in regions.Keys)
            {
                foreach (TRTextileRegion region in regions[tile])
                {
                    if (region.GetTexture(texture) is TRTextileSegment segment)
                    {
                        return new(tile, segment);
                    }
                }
            }
            return null;
        }

        IEnumerable<ushort> laraTextures = meshes
            .SelectMany(m => m.TexturedFaces)
            .Select(f => f.Texture)
            .Distinct();
        foreach (ushort textureIndex in laraTextures)
        {
            TRObjectTexture objectTexture = allTextures[textureIndex];
            Tuple<TRTextile, TRTextileSegment> match = FindSegment(textureIndex);
            Assert.IsNotNull(match);
            Assert.AreEqual(objectTexture.Atlas, match.Item1.Index);
            Assert.AreEqual(textureIndex, match.Item2.Index);
            Assert.AreEqual(objectTexture, match.Item2.Texture);
        }
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test locating sprite texture regions in TR1.")]
    public void TestGetTR1SpriteRegions()
    {
        TR1Level level = GetTR1TestLevel();
        TR1TexturePacker packer = new(level, 16);

        TestGetSpriteRegions(packer, level.Sprites[TR1Type.FontGraphics_S_H]);
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test locating sprite texture regions in TR2.")]
    public void TestGetTR2SpriteRegions()
    {
        TR2Level level = GetTR2TestLevel();
        TR2TexturePacker packer = new(level, 16);

        TestGetSpriteRegions(packer, level.Sprites[TR2Type.FontGraphics_S_H]);
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test locating sprite texture regions in TR3.")]
    public void TestGetTR3SpriteRegions()
    {
        TR3Level level = GetTR3TestLevel();
        TR3TexturePacker packer = new(level, 16);

        TestGetSpriteRegions(packer, level.Sprites[TR3Type.FontGraphics_S_H]);
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test locating sprite texture regions in TR4.")]
    public void TestGetTR4SpriteRegions()
    {
        TR4Level level = GetTR4TestLevel();
        TR4TexturePacker packer = new(level, TRGroupPackingMode.Object, 32);

        TestGetSpriteRegions(packer, level.Sprites[TR4Type.DefaultSprites]);
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test locating sprite texture regions in TR5.")]
    public void TestGetTR5SpriteRegions()
    {
        TR5Level level = GetTR5TestLevel();
        TR5TexturePacker packer = new(level, TRGroupPackingMode.Object, 32);

        TestGetSpriteRegions(packer, level.Sprites[TR5Type.SkyGraphics]);
    }

    private static void TestGetSpriteRegions(TRTexturePacker packer, TRSpriteSequence sequence)
    {
        Assert.AreNotEqual(0, sequence.Textures.Count);
        Dictionary<TRTextile, List<TRTextileRegion>> regions = packer.GetSpriteRegions(sequence);

        List<TRSpriteTexture> textures = new(sequence.Textures);
        foreach (TRTextile tile in regions.Keys)
        {
            foreach (TRTextileRegion region in regions[tile])
            {
                Assert.AreEqual(1, region.Segments.Count);
                Assert.IsTrue(region.Segments[0].Texture is TRSpriteTexture);

                TRSpriteTexture texture = region.Segments[0].Texture as TRSpriteTexture;
                Assert.IsTrue(textures.Contains(texture));
                Assert.AreEqual(texture.Atlas, tile.Index);

                textures.Remove(texture);
            }
        }

        // Did we find them all?
        Assert.AreEqual(0, textures.Count);
    }

    [TestMethod]
    public void TestObjectRemoval()
    {
        TR1Level level = GetTR1TestLevel();
        TR1TexturePacker packer = new(level, 16);

        TRModel lara = level.Models[TR1Type.Lara];
        var regions = packer.GetMeshRegions(lara.Meshes);
        Assert.AreNotEqual(0, regions.Count);

        IEnumerable<int> textures = lara.Meshes
            .SelectMany(m => m.TexturedFaces.Select(t => (int)t.Texture))
            .Distinct();

        packer.RemoveObjectRegions(textures);

        regions = packer.GetMeshRegions(lara.Meshes);
        Assert.AreEqual(0, regions.Count);

        regions = packer.GetObjectRegions(textures);
        Assert.AreEqual(0, regions.Count);
    }

    [TestMethod]
    public void TestSpriteRemoval()
    {
        TR1Level level = GetTR1TestLevel();
        TR1TexturePacker packer = new(level, 16);

        List<TRSpriteSequence> font = new() { level.Sprites[TR1Type.FontGraphics_S_H] };
        var regions = packer.GetSpriteRegions(font);
        Assert.AreNotEqual(0, regions.Count);

        packer.RemoveSpriteRegions(font);

        regions = packer.GetSpriteRegions(font);
        Assert.AreEqual(0, regions.Count);
    }
}
