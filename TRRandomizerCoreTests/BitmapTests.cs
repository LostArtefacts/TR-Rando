using System.Drawing;
using System.Drawing.Imaging;
using TRImageControl;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCoreTests;

[TestClass]
[TestCategory("Bitmaps")]
public class BitmapTests
{
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

        TRImage image = ImageUtilities.BitmapToImage(bitmap1);
        Bitmap bitmap2 = ImageUtilities.ImageToBitmap(image);

        using MemoryStream ms2 = new();
        bitmap2.Save(ms2, ImageFormat.Png);

        CollectionAssert.AreEqual(ms1.ToArray(), ms2.ToArray());
    }
}
