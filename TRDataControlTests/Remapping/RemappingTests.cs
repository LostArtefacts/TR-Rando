using TRDataControl;
using TRImageControl;
using TRImageControl.Packing;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControlTests;

namespace TRDataControlTests.IO;

[TestClass]
[TestCategory("OriginalIO")]
public class RemappingTests : TestBase
{
    [TestMethod]
    [Description("Test that object textures are correct after removing data.")]
    public void TestTextureRemoval()
    {
        TR1Level level = GetTR1Level(TR1LevelNames.CAVES);
        IEnumerable<TRFace> allFaces = level.Rooms.Select(r => r.Mesh).SelectMany(m => m.Faces)
            .Concat(level.DistinctMeshes.SelectMany(m => m.TexturedFaces));

        string GetFaceID(TRFace face)
        {
            TRObjectTexture texInfo = level.ObjectTextures[face.Texture];
            TRImage tile = new(level.Images8[texInfo.Atlas].Pixels, level.Palette);
            return tile.Export(texInfo.Bounds).GenerateID();
        }

        Dictionary<TRFace, string> originalIDs = new();
        foreach (TRFace face in allFaces)
        {
            originalIDs[face] = GetFaceID(face);
        }

        List<int> staleTextures = new(level.Models[TR1Type.Bear].Meshes
            .SelectMany(m => m.TexturedFaces.Select(t => (int)t.Texture)).Distinct());
        level.Models.Remove(TR1Type.Bear);

        TR1TextureRemapper remapper = new(level);
        remapper.RemoveUnusedTextures(staleTextures);

        foreach (TRFace face in allFaces)
        {
            Assert.AreEqual(originalIDs[face], GetFaceID(face));
        }
    }

    [TestMethod]
    [Description("As above, but with an added dependency on a different model.")]
    public void TestDependencies()
    {
        TR1Level level = GetTR1Level(TR1LevelNames.CAVES);

        // Simulate a shared texture
        ushort sharedTexture = level.Models[TR1Type.Bear].Meshes[0].TexturedRectangles[0].Texture;
        level.Models[TR1Type.Wolf].Meshes[0].TexturedRectangles[0].Texture = sharedTexture;

        IEnumerable<TRFace> allFaces = level.Rooms.Select(r => r.Mesh).SelectMany(m => m.Faces)
            .Concat(level.DistinctMeshes.SelectMany(m => m.TexturedFaces));

        string GetFaceID(TRFace face)
        {
            TRObjectTexture texInfo = level.ObjectTextures[face.Texture];
            TRImage tile = new(level.Images8[texInfo.Atlas].Pixels, level.Palette);
            return tile.Export(texInfo.Bounds).GenerateID();
        }

        Dictionary<TRFace, string> originalIDs = new();
        foreach (TRFace face in allFaces)
        {
            originalIDs[face] = GetFaceID(face);
        }

        List<int> staleTextures = new(level.Models[TR1Type.Bear].Meshes
            .SelectMany(m => m.TexturedFaces.Select(t => (int)t.Texture)).Distinct());
        level.Models.Remove(TR1Type.Bear);

        TR1TextureRemapGroup remapGroup = new()
        {
            Dependencies = new()
            {
                new()
                {
                    TileIndex = level.ObjectTextures[sharedTexture].Atlas,
                    Bounds = level.ObjectTextures[sharedTexture].Bounds,
                    Types = new() { TR1Type.Bear, TR1Type.Wolf }
                }
            }
        };

        TR1TextureRemapper remapper = new(level);
        remapper.RemoveUnusedTextures(staleTextures,
            (tile, bounds) => remapGroup.CanRemoveRectangle(tile, bounds, new List<TR1Type>() { TR1Type.Bear }));

        foreach (TRFace face in allFaces)
        {
            Assert.AreEqual(originalIDs[face], GetFaceID(face));
        }
    }
}
