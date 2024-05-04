using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using TRLevelControl.Model;

namespace TRLevelControlTests.Common;

[TestClass]
[TestCategory("Textures")]
public partial class TextureTests
{
    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWClockwise UV mode is properly detected on a quad.")]
    public void TestUVDetect_NWClockwiseQuad()
    {
        TRObjectTexture texture = MakeNWClockwiseQuad();
        Assert.IsFalse(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NW_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWClockwise UV mode is properly detected on a tri (variant 1).")]
    public void TestUVDetect_NWClockwiseTri1()
    {
        TRObjectTexture texture = MakeNWClockwiseTri1();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NW_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWClockwise UV mode is properly detected on a tri (variant 2).")]
    public void TestUVDetect_NWClockwiseTri2()
    {
        TRObjectTexture texture = MakeNWClockwiseTri2();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NW_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWClockwise UV mode is properly detected on a tri (variant 3).")]
    public void TestUVDetect_NWClockwiseTri3()
    {
        TRObjectTexture texture = MakeNWClockwiseTri3();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NW_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEClockwise UV mode is properly detected on a quad.")]
    public void TestUVDetect_NEClockwiseQuad()
    {
        TRObjectTexture texture = MakeNEClockwiseQuad();
        Assert.IsFalse(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NE_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEClockwise UV mode is properly detected on a tri (variant 1).")]
    public void TestUVDetect_NEClockwiseTri1()
    {
        TRObjectTexture texture = MakeNEClockwiseTri1();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NE_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEClockwise UV mode is properly detected on a tri (variant 2).")]
    public void TestUVDetect_NEClockwiseTri2()
    {
        TRObjectTexture texture = MakeNEClockwiseTri2();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NE_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEClockwise UV mode is properly detected on a tri (variant 3).")]
    public void TestUVDetect_NEClockwiseTri3()
    {
        TRObjectTexture texture = MakeNEClockwiseTri3();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NE_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEClockwise UV mode is properly detected on a quad.")]
    public void TestUVDetect_SEClockwiseQuad()
    {
        TRObjectTexture texture = MakeSEClockwiseQuad();
        Assert.IsFalse(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SE_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEClockwise UV mode is properly detected on a tri (variant 1).")]
    public void TestUVDetect_SEClockwiseTri1()
    {
        TRObjectTexture texture = MakeSEClockwiseTri1();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SE_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEClockwise UV mode is properly detected on a tri (variant 2).")]
    public void TestUVDetect_SEClockwiseTri2()
    {
        TRObjectTexture texture = MakeSEClockwiseTri2();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SE_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEClockwise UV mode is properly detected on a tri (variant 3).")]
    public void TestUVDetect_SEClockwiseTri3()
    {
        TRObjectTexture texture = MakeSEClockwiseTri3();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SE_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWClockwise UV mode is properly detected on a quad.")]
    public void TestUVDetect_SWClockwiseQuad()
    {
        TRObjectTexture texture = MakeSWClockwiseQuad();
        Assert.IsFalse(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SW_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWClockwise UV mode is properly detected on a tri (variant 1).")]
    public void TestUVDetect_SWClockwiseTri1()
    {
        TRObjectTexture texture = MakeSWClockwiseTri1();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SW_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWClockwise UV mode is properly detected on a tri (variant 2).")]
    public void TestUVDetect_SWClockwiseTri2()
    {
        TRObjectTexture texture = MakeSWClockwiseTri2();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SW_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWClockwise UV mode is properly detected on a tri (variant 3).")]
    public void TestUVDetect_SWClockwiseTri3()
    {
        TRObjectTexture texture = MakeSWClockwiseTri3();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SW_Clockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWAntiClockwise UV mode is properly detected on a quad.")]
    public void TestUVDetect_NWAntiClockwiseQuad()
    {
        TRObjectTexture texture = MakeNWAntiClockwiseQuad();
        Assert.IsFalse(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NW_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWAntiClockwise UV mode is properly detected on a tri (variant 1).")]
    public void TestUVDetect_NWAntiClockwiseTri1()
    {
        TRObjectTexture texture = MakeNWAntiClockwiseTri1();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NW_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWAntiClockwise UV mode is properly detected on a tri (variant 2).")]
    public void TestUVDetect_NWAntiClockwiseTri2()
    {
        TRObjectTexture texture = MakeNWAntiClockwiseTri2();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NW_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWAntiClockwise UV mode is properly detected on a tri (variant 3).")]
    public void TestUVDetect_NWAntiClockwiseTri3()
    {
        TRObjectTexture texture = MakeNWAntiClockwiseTri3();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NW_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEAntiClockwise UV mode is properly detected on a qaud.")]
    public void TestUVDetect_NEAntiClockwiseQuad()
    {
        TRObjectTexture texture = MakeNEAntiClockwiseQuad();
        Assert.IsFalse(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NE_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEAntiClockwise UV mode is properly detected on a tri (variant 1).")]
    public void TestUVDetect_NEAntiClockwiseTri1()
    {
        TRObjectTexture texture = MakeNEAntiClockwiseTri1();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NE_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEAntiClockwise UV mode is properly detected on a tri (variant 2).")]
    public void TestUVDetect_NEAntiClockwiseTri2()
    {
        TRObjectTexture texture = MakeNEAntiClockwiseTri2();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NE_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEAntiClockwise UV mode is properly detected on a tri (variant 3).")]
    public void TestUVDetect_NEAntiClockwiseTri3()
    {
        TRObjectTexture texture = MakeNEAntiClockwiseTri3();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.NE_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEAntiClockwise UV mode is properly detected on a quad.")]
    public void TestUVDetect_SEAntiClockwiseQuad()
    {
        TRObjectTexture texture = MakeSEAntiClockwiseQuad();
        Assert.IsFalse(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SE_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEAntiClockwise UV mode is properly detected on a tri (variant 1).")]
    public void TestUVDetect_SEAntiClockwiseTri1()
    {
        TRObjectTexture texture = MakeSEAntiClockwiseTri1();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SE_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEAntiClockwise UV mode is properly detected on a tri (variant 2).")]
    public void TestUVDetect_SEAntiClockwiseTri2()
    {
        TRObjectTexture texture = MakeSEAntiClockwiseTri2();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SE_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEAntiClockwise UV mode is properly detected on a tri (variant 3).")]
    public void TestUVDetect_SEAntiClockwiseTri3()
    {
        TRObjectTexture texture = MakeSEAntiClockwiseTri3();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SE_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWAntiClockwise UV mode is properly detected on a quad.")]
    public void TestUVDetect_SWAntiClockwiseQuad()
    {
        TRObjectTexture texture = MakeSWAntiClockwiseQuad();
        Assert.IsFalse(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SW_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWAntiClockwise UV mode is properly detected on a tri (variant 1).")]
    public void TestUVDetect_SWAntiClockwiseTri1()
    {
        TRObjectTexture texture = MakeSWAntiClockwiseTri1();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SW_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWAntiClockwise UV mode is properly detected on a tri (variant 2).")]
    public void TestUVDetect_SWAntiClockwiseTri2()
    {
        TRObjectTexture texture = MakeSWAntiClockwiseTri2();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SW_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWAntiClockwise UV mode is properly detected on a tri (variant 3).")]
    public void TestUVDetect_SWAntiClockwiseTri3()
    {
        TRObjectTexture texture = MakeSWAntiClockwiseTri3();
        Assert.IsTrue(texture.HasTriangleVertex);
        Assert.AreEqual(TRUVMode.SW_AntiClockwise, texture.UVMode);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWClockwise UV mode converts properly to every other mode (quad).")]
    public void TestUVSwitch_NWClockwiseQuad()
    {
        TRObjectTexture texture = MakeNWClockwiseQuad();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWClockwise UV mode converts properly to every other mode (tri1).")]
    public void TestUVSwitch_NWClockwiseTri1()
    {
        TRObjectTexture texture = MakeNWClockwiseTri1();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWClockwise UV mode converts properly to every other mode (tri2).")]
    public void TestUVSwitch_NWClockwiseTri2()
    {
        TRObjectTexture texture = MakeNWClockwiseTri2();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWClockwise UV mode converts properly to every other mode (tri3).")]
    public void TestUVSwitch_NWClockwiseTri3()
    {
        TRObjectTexture texture = MakeNWClockwiseTri3();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEClockwise UV mode converts properly to every other mode (quad).")]
    public void TestUVSwitch_NEClockwiseQuad()
    {
        TRObjectTexture texture = MakeNEClockwiseQuad();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEClockwise UV mode converts properly to every other mode (tri1).")]
    public void TestUVSwitch_NEClockwiseTri1()
    {
        TRObjectTexture texture = MakeNEClockwiseTri1();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEClockwise UV mode converts properly to every other mode (tri2).")]
    public void TestUVSwitch_NEClockwiseTri2()
    {
        TRObjectTexture texture = MakeNEClockwiseTri2();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEClockwise UV mode converts properly to every other mode (tri3).")]
    public void TestUVSwitch_NEClockwiseTri3()
    {
        TRObjectTexture texture = MakeNEClockwiseTri3();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEClockwise UV mode converts properly to every other mode (quad).")]
    public void TestUVSwitch_SEClockwiseQuad()
    {
        TRObjectTexture texture = MakeSEClockwiseQuad();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEClockwise UV mode converts properly to every other mode (tri1).")]
    public void TestUVSwitch_SEClockwiseTri1()
    {
        TRObjectTexture texture = MakeSEClockwiseTri1();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEClockwise UV mode converts properly to every other mode (tri2).")]
    public void TestUVSwitch_SEClockwiseTri2()
    {
        TRObjectTexture texture = MakeSEClockwiseTri2();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEClockwise UV mode converts properly to every other mode (tri2).")]
    public void TestUVSwitch_SEClockwiseTri3()
    {
        TRObjectTexture texture = MakeSEClockwiseTri3();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWClockwise UV mode converts properly to every other mode (quad).")]
    public void TestUVSwitch_SWClockwiseQuad()
    {
        TRObjectTexture texture = MakeSWClockwiseQuad();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWClockwise UV mode converts properly to every other mode (tri1).")]
    public void TestUVSwitch_SWClockwiseTri1()
    {
        TRObjectTexture texture = MakeSWClockwiseTri1();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWClockwise UV mode converts properly to every other mode (tri2).")]
    public void TestUVSwitch_SWClockwiseTri2()
    {
        TRObjectTexture texture = MakeSWClockwiseTri2();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWClockwise UV mode converts properly to every other mode (tri3).")]
    public void TestUVSwitch_SWClockwiseTri3()
    {
        TRObjectTexture texture = MakeSWClockwiseTri3();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWAntiClockwise UV mode converts properly to every other mode (quad).")]
    public void TestUVSwitch_NWAntiClockwiseQuad()
    {
        TRObjectTexture texture = MakeNWAntiClockwiseQuad();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWAntiClockwise UV mode converts properly to every other mode (tri1).")]
    public void TestUVSwitch_NWAntiClockwiseTri1()
    {
        TRObjectTexture texture = MakeNWAntiClockwiseTri1();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWAntiClockwise UV mode converts properly to every other mode (tri2).")]
    public void TestUVSwitch_NWAntiClockwiseTri2()
    {
        TRObjectTexture texture = MakeNWAntiClockwiseTri2();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NWAntiClockwise UV mode converts properly to every other mode (tri3).")]
    public void TestUVSwitch_NWAntiClockwiseTri3()
    {
        TRObjectTexture texture = MakeNWAntiClockwiseTri3();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEAntiClockwise UV mode converts properly to every other mode (quad).")]
    public void TestUVSwitch_NEAntiClockwiseQuad()
    {
        TRObjectTexture texture = MakeNEAntiClockwiseQuad();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEAntiClockwise UV mode converts properly to every other mode (tri1).")]
    public void TestUVSwitch_NEAntiClockwiseTri1()
    {
        TRObjectTexture texture = MakeNEAntiClockwiseTri1();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEAntiClockwise UV mode converts properly to every other mode (tri2).")]
    public void TestUVSwitch_NEAntiClockwiseTri2()
    {
        TRObjectTexture texture = MakeNEAntiClockwiseTri2();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that NEAntiClockwise UV mode converts properly to every other mode (tri3).")]
    public void TestUVSwitch_NEAntiClockwiseTri3()
    {
        TRObjectTexture texture = MakeNEAntiClockwiseTri3();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEAntiClockwise UV mode converts properly to every other mode (quad).")]
    public void TestUVSwitch_SEAntiClockwiseQuad()
    {
        TRObjectTexture texture = MakeSEAntiClockwiseQuad();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEAntiClockwise UV mode converts properly to every other mode (tri1).")]
    public void TestUVSwitch_SEAntiClockwiseTri1()
    {
        TRObjectTexture texture = MakeSEAntiClockwiseTri1();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEAntiClockwise UV mode converts properly to every other mode (tri1).")]
    public void TestUVSwitch_SEAntiClockwiseTri2()
    {
        TRObjectTexture texture = MakeSEAntiClockwiseTri2();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SEAntiClockwise UV mode converts properly to every other mode (tri3).")]
    public void TestUVSwitch_SEAntiClockwiseTri3()
    {
        TRObjectTexture texture = MakeSEAntiClockwiseTri3();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWAntiClockwise UV mode converts properly to every other mode (quad).")]
    public void TestUVSwitch_SWAntiClockwiseQuad()
    {
        TRObjectTexture texture = MakeSWAntiClockwiseQuad();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWAntiClockwise UV mode converts properly to every other mode (tri1).")]
    public void TestUVSwitch_SWAntiClockwiseTri1()
    {
        TRObjectTexture texture = MakeSWAntiClockwiseTri1();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWAntiClockwise UV mode converts properly to every other mode (tri2).")]
    public void TestUVSwitch_SWAntiClockwiseTri2()
    {
        TRObjectTexture texture = MakeSWAntiClockwiseTri2();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that SWAntiClockwise UV mode converts properly to every other mode (tri3).")]
    public void TestUVSwitch_SWAntiClockwiseTri3()
    {
        TRObjectTexture texture = MakeSWAntiClockwiseTri3();
        SwitchMode(texture);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that bounds are accurately calculated.")]
    public void TestTextureBounds()
    {
        TRObjectTexture texture = new()
        {
            Vertices = new()
            {
                new(64, 128),
                new(95, 128),
                new(95, 143),
                new(64, 143)
            }
        };

        Assert.AreEqual(64, texture.Position.X);
        Assert.AreEqual(128, texture.Position.Y);
        Assert.AreEqual(32, texture.Size.Width);
        Assert.AreEqual(16, texture.Size.Height);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that bounds are accurately calculated after moving a texture.")]
    public void TestTextureReposition()
    {
        TRObjectTexture texture = new()
        {
            Vertices = new()
            {
                new(64, 128),
                new(95, 128),
                new(95, 143),
                new(64, 143)
            }
        };

        Assert.AreEqual(64, texture.Position.X);
        Assert.AreEqual(128, texture.Position.Y);

        texture.Position = new(192, 240);

        Assert.AreEqual(192, texture.Position.X);
        Assert.AreEqual(240, texture.Position.Y);
        Assert.AreEqual(32, texture.Size.Width);
        Assert.AreEqual(16, texture.Size.Height);
    }

    [TestMethod]
    [TestCategory("Textures")]
    [Description("Test that bounds are accurately calculated after creating a default texture.")]
    public void TestTextureCreate()
    {
        Rectangle bounds = new(64, 128, 128, 48);
        TRObjectTexture texture = new(bounds);

        Assert.AreEqual(bounds.X, texture.Bounds.X);
        Assert.AreEqual(bounds.Y, texture.Bounds.Y);
        Assert.AreEqual(bounds.Width, texture.Bounds.Width);
        Assert.AreEqual(bounds.Height, texture.Bounds.Height);
    }

    [TestMethod]
    public void TestFlipHorizontal()
    {
        static void Test(TRObjectTexture texture, TRUVMode expected)
        {
            texture.FlipHorizontal();
            Assert.AreEqual(expected, texture.UVMode);
        }

        Test(MakeNWClockwiseQuad(), TRUVMode.NE_AntiClockwise);
        Test(MakeNEClockwiseQuad(), TRUVMode.NW_AntiClockwise);
        Test(MakeSEClockwiseQuad(), TRUVMode.SW_AntiClockwise);
        Test(MakeSWClockwiseQuad(), TRUVMode.SE_AntiClockwise);
        Test(MakeNWAntiClockwiseQuad(), TRUVMode.NE_Clockwise);
        Test(MakeNEAntiClockwiseQuad(), TRUVMode.NW_Clockwise);
        Test(MakeSEAntiClockwiseQuad(), TRUVMode.SW_Clockwise);
        Test(MakeSWAntiClockwiseQuad(), TRUVMode.SE_Clockwise);
    }

    [TestMethod]
    public void TestFlipVertical()
    {
        static void Test(TRObjectTexture texture, TRUVMode expected)
        {
            texture.FlipVertical();
            Assert.AreEqual(expected, texture.UVMode);
        }

        Test(MakeNWClockwiseQuad(), TRUVMode.SW_AntiClockwise);
        Test(MakeNEClockwiseQuad(), TRUVMode.SE_AntiClockwise);
        Test(MakeSEClockwiseQuad(), TRUVMode.NE_AntiClockwise);
        Test(MakeSWClockwiseQuad(), TRUVMode.NW_AntiClockwise);
        Test(MakeNWAntiClockwiseQuad(), TRUVMode.SW_Clockwise);
        Test(MakeNEAntiClockwiseQuad(), TRUVMode.SE_Clockwise);
        Test(MakeSEAntiClockwiseQuad(), TRUVMode.NE_Clockwise);
        Test(MakeSWAntiClockwiseQuad(), TRUVMode.NW_Clockwise);
    }

    private static void SwitchMode(TRObjectTexture texture)
    {
        TRUVMode originalMode = texture.UVMode;
        foreach (TRUVMode mode in Enum.GetValues<TRUVMode>())
        {
            texture.UVMode = mode;
            Assert.AreEqual(mode, texture.UVMode);
            texture.UVMode = originalMode;
        }
    }
}
