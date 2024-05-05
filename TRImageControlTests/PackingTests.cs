using RectanglePacker.Events;
using System.Drawing;
using TRImageControl.Packing;
using TRLevelControl;
using TRLevelControl.Model;
using TRLevelControlTests;

namespace TRImageControlTests;

[TestClass]
public class PackingTests : TestBase
{
    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test texture packing in TR1.")]
    public void TestTR1Packing()
    {
        TR1Level level1 = GetTR1TestLevel();
        TR1Level level2 = GetTR1AltTestLevel();

        TR1TexturePacker packer1 = new(level1, 16);
        TR1TexturePacker packer2 = new(level2, 16);

        TestPacking(packer1, packer2);
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test texture packing in TR2.")]
    public void TestTR2Packing()
    {
        TR2Level level1 = GetTR2TestLevel();
        TR2Level level2 = GetTR2AltTestLevel();

        TR2TexturePacker packer1 = new(level1, 16);
        TR2TexturePacker packer2 = new(level2, 16);

        TestPacking(packer1, packer2);
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test texture packing in TR3.")]
    public void TestTR3Packing()
    {
        TR3Level level1 = GetTR3TestLevel();
        TR3Level level2 = GetTR3AltTestLevel();

        TR3TexturePacker packer1 = new(level1, 16);
        TR3TexturePacker packer2 = new(level2, 16);

        TestPacking(packer1, packer2);
    }

    private static void TestPacking(TRTexturePacker packer1, TRTexturePacker packer2)
    {
        Assert.AreNotEqual(0, packer2.Tiles[0].Rectangles.Count);
        packer1.AddRectangles(packer2.Tiles[0].Rectangles);
        PackingResult<TRTextile, TRTextileRegion> result = packer1.Pack(true);

        Assert.AreEqual(0, result.OrphanCount);

        IEnumerable<TRTextileRegion> expectedRegions = packer2.Tiles[0].Rectangles;
        IEnumerable<TRTextileRegion> allRegions = packer1.Tiles.SelectMany(t => t.Rectangles);

        Assert.IsTrue(expectedRegions.All(allRegions.Contains));
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test that packing fails when there is not enough space.")]
    public void TestPackingFailure()
    {
        TR1Level level1 = GetTR1TestLevel();
        TR1Level level2 = GetTR1AltTestLevel();

        TR1TexturePacker packer1 = new(level1, level1.Images8.Count + 1);
        TR1TexturePacker packer2 = new(level2, level2.Images8.Count);

        packer1.AddRectangles(packer2.Tiles.SelectMany(t => t.Rectangles));

        try
        {
            packer1.Pack(true);
            Assert.Fail();
        }
        catch (PackingException) { }
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test adding similar segments to a textile results in the correct region allocation.")]
    public void TestSimilarSegmentAddition()
    {
        TRTextile textile = new()
        {
            Width = TRConsts.TPageWidth,
            Height = TRConsts.TPageHeight,
            Image = new(new Size(TRConsts.TPageWidth, TRConsts.TPageHeight))
        };
        Assert.AreEqual(0, textile.Rectangles.Count);

        TRTextileSegment segment1 = new()
        {
            Texture = new TRObjectTexture(0, 0, 64, 48)
        };
        textile.AddSegment(segment1);
        Assert.AreEqual(1, textile.Rectangles.Count);

        TRTextileSegment segment2 = new()
        {
            Texture = new TRObjectTexture(0, 0, 32, 16)
        };
        textile.AddSegment(segment2);
        Assert.AreEqual(1, textile.Rectangles.Count);

        TRTextileRegion region = textile.Rectangles[0];
        Assert.AreEqual(2, region.Segments.Count);
        Assert.IsTrue(region.Segments.Contains(segment1));
        Assert.IsTrue(region.Segments.Contains(segment2));
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test adding dissimilar segments to a textile results in the correction region allocation.")]
    public void TestDissimilarSegmentAddition()
    {
        TRTextile textile = new()
        {
            Width = TRConsts.TPageWidth,
            Height = TRConsts.TPageHeight,
            Image = new(new Size(TRConsts.TPageWidth, TRConsts.TPageHeight))
        };
        Assert.AreEqual(0, textile.Rectangles.Count);

        TRTextileSegment segment1 = new()
        {
            Texture = new TRObjectTexture(0, 0, 64, 48)
        };
        textile.AddSegment(segment1);
        Assert.AreEqual(1, textile.Rectangles.Count);

        TRTextileSegment segment2 = new()
        {
            Texture = new TRObjectTexture(64, 0, 32, 16)
        };
        textile.AddSegment(segment2);
        Assert.AreEqual(2, textile.Rectangles.Count);

        TRTextileRegion region1 = textile.Rectangles[0];
        Assert.AreEqual(1, region1.Segments.Count);
        Assert.IsTrue(region1.Segments.Contains(segment1));

        TRTextileRegion region2 = textile.Rectangles[1];
        Assert.AreEqual(1, region2.Segments.Count);
        Assert.IsTrue(region2.Segments.Contains(segment2));
    }

    [TestMethod]
    [TestCategory("Packing")]
    [Description("Test moving a region from one tile to another.")]
    public void TestRegionMove()
    {
        // Make a red tile with a 64x48 region
        TRTextile textile1 = new()
        {
            Width = TRConsts.TPageWidth,
            Height = TRConsts.TPageHeight,
            Image = new(new Size(TRConsts.TPageWidth, TRConsts.TPageHeight))
        };
        textile1.Image.Write((c, x, y) => Color.Red);

        Rectangle testBounds = new(0, 0, 64, 48);
        textile1.AddSegment(new()
        {
            Texture = new TRObjectTexture(testBounds)
        });

        // Make a blue tile with a 64x48 region
        TRTextile textile2 = new()
        {
            Width = TRConsts.TPageWidth,
            Height = TRConsts.TPageHeight,
            Image = new(new Size(TRConsts.TPageWidth, TRConsts.TPageHeight))
        };
        textile2.Image.Write((c, x, y) => Color.Blue);

        textile2.AddSegment(new()
        {
            Texture = new TRObjectTexture(testBounds)
        });

        Assert.AreEqual(1, textile1.Rectangles.Count);
        Assert.AreEqual(1, textile2.Rectangles.Count);

        // Move the red region to the blue tile
        TRTextileRegion region = textile1.Rectangles[0];
        textile1.Remove(region);
        textile2.Add(region);

        Assert.AreEqual(0, textile1.Rectangles.Count);
        Assert.AreEqual(2, textile2.Rectangles.Count);

        // Test that where the region used to be is now transparent and
        // everything else is still red.
        textile1.Image.Read((c, x, y) =>
        {
            if (testBounds.Contains(new Point(x, y)))
            {
                Assert.AreEqual(0, c.A);
            }
            else
            {
                Assert.AreEqual(Color.Red.R, c.R);
                Assert.AreEqual(Color.Red.G, c.G);
                Assert.AreEqual(Color.Red.B, c.B);
            }
        });

        // Test that where the region is now is all red and everything else
        // is still blue.
        textile2.Image.Read((c, x, y) =>
        {
            if (region.MappedBounds.Contains(new Point(x, y)))
            {
                Assert.AreEqual(Color.Red.R, c.R);
                Assert.AreEqual(Color.Red.G, c.G);
                Assert.AreEqual(Color.Red.B, c.B);
            }
            else
            {
                Assert.AreEqual(Color.Blue.R, c.R);
                Assert.AreEqual(Color.Blue.G, c.G);
                Assert.AreEqual(Color.Blue.B, c.B);
            }
        });
    }
}
