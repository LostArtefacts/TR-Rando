using System.Drawing;
using System.Drawing.Imaging;
using TRImageControl;
using TRLevelControl.Model;
using TRLevelControlTests;

namespace TRImageControlTests;

[TestClass]
[TestCategory("Textures")]
public class ImageTests : TestBase
{
    [TestMethod]
    [Description("Test that an 8-bit image is identical after conversion into TRImage type.")]
    public void Test8Bit()
    {
        TR1Level level = GetTR1TestLevel();

        TRTexImage8 tex = level.Images8[^1];
        TRImage img = new(tex.Pixels, level.Palette);
        byte[] pixels = img.ToRGB(level.Palette);

        List<TRColour> colours1 = tex.Pixels.Select(p => level.Palette[p]).ToList();
        List<TRColour> colours2 = pixels.Select(p => level.Palette[p]).ToList();

        Assert.AreEqual(colours1.Count, colours2.Count);
        for (int i = 0; i < colours1.Count; i++)
        {
            TRColour c1 = colours1[i];
            TRColour c2 = colours2[i];
            Assert.AreEqual(c1.Red, c2.Red);
            Assert.AreEqual(c1.Green, c2.Green);
            Assert.AreEqual(c1.Blue, c2.Blue);
        }
    }

    [TestMethod]
    [Description("Test that a 16-bit image is identical after conversion into TRImage type.")]
    public void Test16Bit()
    {
        TR2Level level = GetTR2TestLevel();

        TRTexImage16 tex = level.Images16[^1];
        TRImage img = new(tex.Pixels);
        ushort[] pixels = img.ToRGB555();

        CollectionAssert.AreEqual(tex.Pixels, pixels);
    }

    [TestMethod]
    [Description("Test that a 32-bit image is identical after conversion into TRImage type.")]
    public void Test32Bit()
    {
        TR4Level level = GetTR4TestLevel();

        TRTexImage32 tex = level.Images.Rooms.Images32[^1];
        TRImage img = new(tex.Pixels);
        uint[] pixels = img.ToRGB32();

        CollectionAssert.AreEqual(tex.Pixels, pixels);
    }

    [TestMethod]
    [Description("Test that the correct portion of an image is deleted.")]
    public void TestDelete()
    {
        TR1Level level = GetTR1TestLevel();

        TRTexImage8 tex = level.Images8[0];
        TRImage original = new(tex.Pixels, level.Palette);
        TRImage edited = new(tex.Pixels, level.Palette);

        Rectangle rect = new(64, 128, 48, 32);

        // Ensure there is no transparency for this test
        Assert.IsFalse(original.Pixels.Any(p => p == 0));

        // Delete the region
        edited.Delete(rect);

        // Ensure there is nothing but transparency where we deleted, and everything else is untouched.
        for (int y = 0; y < original.Size.Height; y++)
        {
            for (int x = 0; x < original.Size.Width; x++)
            {
                if (y >= rect.Top && y < rect.Bottom && x >= rect.Left && x < rect.Right)
                {
                    Assert.AreEqual(0u, edited[x, y]);
                }
                else
                {
                    Assert.AreEqual(original[x, y], edited[x, y]);
                }
            }
        }
    }

    [TestMethod]
    [Description("Test that a portion of an image exported matches the source.")]
    public void TestCopy()
    {
        TR1Level level = GetTR1TestLevel();

        TRTexImage8 tex = level.Images8[0];
        TRImage original = new(tex.Pixels, level.Palette);

        Rectangle rect = new(64, 128, 48, 32);
        TRImage extract = original.Export(rect);

        for (int y = 0; y < extract.Size.Height; y++)
        {
            for (int x = 0; x < extract.Size.Width; x++)
            {
                Assert.AreEqual(original[x + rect.X, y + rect.Y], extract[x, y]);
            }
        }
    }

    [TestMethod]
    [Description("Test importing an image into another.")]
    [DataRow(true)]
    [DataRow(false)]
    public void TestImport(bool retainBackground)
    {
        TR1Level level = GetTR1TestLevel();

        TRImage originalDestination = new(level.Images8[0].Pixels, level.Palette);
        TRImage destination = new(level.Images8[0].Pixels, level.Palette);
        TRImage source = new(level.Images8[1].Pixels, level.Palette);

        Rectangle sourceRect = new(64, 128, 48, 32);
        Rectangle destRect = new(32, 96, 48, 32);

        bool TestMatch()
        {
            for (int y = 0; y < sourceRect.Height; y++)
            {
                for (int x = 0; x < sourceRect.Width; x++)
                {
                    Color src = source.GetPixel(x + sourceRect.X, y + sourceRect.Y);
                    Color dst = destination.GetPixel(x + destRect.X, y + destRect.Y);

                    if (retainBackground && dst.A == 0
                        && dst != originalDestination.GetPixel(x + destRect.X, y + destRect.Y))
                    {
                        return false;
                    }

                    if (src.A == dst.A && src != dst)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // Ensure the destination does not already match the source
        Assert.IsFalse(TestMatch());

        TRImage extract = source.Export(sourceRect);
        destination.Import(extract, destRect.Location, retainBackground);

        // Ensure the destination now does match the source
        Assert.IsTrue(TestMatch());
    }

    [TestMethod]
    [Description("Test reading and writing to an image.")]
    public void TestReadWrite()
    {
        TR1Level level = GetTR1TestLevel();

        TRImage image = new(level.Images8[0].Pixels, level.Palette);

        image.Fill(Color.OrangeRed);

        image.Read((c, x, y) =>
        {
            Assert.AreEqual(Color.OrangeRed.A, c.A);
            Assert.AreEqual(Color.OrangeRed.R, c.R);
            Assert.AreEqual(Color.OrangeRed.G, c.G);
            Assert.AreEqual(Color.OrangeRed.B, c.B);
        });

        Rectangle region = new(64, 32, 48, 16);
        image.Fill(region, Color.BlueViolet);

        image.Read(region, (c, x, y) =>
        {
            Assert.AreEqual(Color.BlueViolet.A, c.A);
            Assert.AreEqual(Color.BlueViolet.R, c.R);
            Assert.AreEqual(Color.BlueViolet.G, c.G);
            Assert.AreEqual(Color.BlueViolet.B, c.B);
        });
    }

    [TestMethod]
    [Description("Test bitmap interpretation.")]
    public void TestBitmap()
    {
        using Bitmap bitmap1 = new(64, 32);
        Graphics graphics = Graphics.FromImage(bitmap1);

        graphics.FillRectangle(new SolidBrush(Color.Blue), new(0, 0, 64, 16));
        graphics.FillRectangle(new SolidBrush(Color.Green), new(0, 16, 64, 16));
        graphics.DrawLine(new(Color.Red, 1), new(10, 10), new(20, 20));

        using MemoryStream ms1 = new();
        bitmap1.Save(ms1, ImageFormat.Png);

        TRImage image = new(bitmap1);
        Bitmap bitmap2 = image.ToBitmap();

        using MemoryStream ms2 = new();
        bitmap2.Save(ms2, ImageFormat.Png);

        CollectionAssert.AreEqual(ms1.ToArray(), ms2.ToArray());
    }

    [TestMethod]
    [Description("Test alpha blending")]
    public void TestBlending()
    {
        TRImage baseImage = new(128, 128);
        baseImage.Fill(Color.FromArgb(153, 0, 100, 255)); // 60%

        TRImage overlay = new(64, 64);
        overlay.Fill(Color.FromArgb(102, Color.Red)); // 40%

        baseImage.Import(overlay, new(32, 32), true);

        Color blend = baseImage.GetPixel(32, 32);
        Assert.AreEqual(134, blend.R);
        Assert.AreEqual(47, blend.G);
        Assert.AreEqual(120, blend.B);
        Assert.AreEqual(193, blend.A);
    }

    [TestMethod]
    public void TestDDS()
    {
        TRImage image = new(512, 256);
        image.Fill(Color.Red);
        image.Save("test.dds");

        // Verify it's not PNG
        File.Move("test.dds", "test.png", true);
        try
        {
            image = new("test.png");
            Assert.Fail();
        }
        catch { }

        // Basic check that it's readable
        File.Move("test.png", "test.dds", true);
        image = new("test.dds");
        
        Assert.AreEqual(512, image.Width);
        Assert.AreEqual(256, image.Height);
    }
}
