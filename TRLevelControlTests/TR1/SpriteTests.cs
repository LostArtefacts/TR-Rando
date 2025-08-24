using TRLevelControl.Model;

namespace TRLevelControlTests.TR1;

[TestClass]
[TestCategory("Sprites")]
public class SpriteTests : TestBase
{
    [TestMethod]
    public void TestRoomSprites()
    {
        TR1Level level = GetTR1AltTestLevel();
        Assert.IsTrue(level.Rooms[0].Mesh.Sprites.Count == 1);
        TRRoomSprite<TR1Type> sprite = level.Rooms[0].Mesh.Sprites[0];

        Assert.AreEqual(TR1Type.Plant2, sprite.ID);
        Assert.AreEqual(0, sprite.Frame);

        Assert.IsTrue(level.Sprites[TR1Type.Plant2].Textures.Count > 1);
        sprite.Frame = (short)(level.Sprites[TR1Type.Plant2].Textures.Count - 1);

        level = WriteReadTempLevel(level);
        sprite = level.Rooms[0].Mesh.Sprites[0];

        Assert.AreEqual(TR1Type.Plant2, sprite.ID);
        Assert.AreEqual((short)(level.Sprites[TR1Type.Plant2].Textures.Count - 1), sprite.Frame);
    }
}
