using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR5;

[TestClass]
[TestCategory("OriginalIO")]
public class OGLightTests : TestBase
{
    [TestMethod]
    [Description("Test lights in a room that does not have fog bulbs.")]
    public void TestLightsNoFog()
    {
        TR5Level level = GetTR5Level(TR5LevelNames.MARKETS);

        void Test()
        {
            TR5Room room = level.Rooms[203];
            Assert.AreEqual(0, room.Lights.FindAll(l => l is TR5FogBulb).Count);
            Assert.AreEqual(4, room.Lights.Count);

            Assert.IsTrue(room.Lights[0] is TR5SunLight);
            Assert.IsTrue(room.Lights[1] is TR5SpotLight);
            Assert.IsTrue(room.Lights[2] is TR5ShadowLight);
            Assert.IsTrue(room.Lights[3] is TR5PointLight);
        }

        // Initial read test
        Test();

        // Ensure the same after write
        level = WriteReadTempLevel(level);
        Test();
    }

    [TestMethod]
    [Description("Test fog bulbs in a room that has other lights.")]
    public void TestFogBulbsWithLights()
    {
        TR5Level level = GetTR5Level(TR5LevelNames.ROME);

        void Test()
        {
            TR5Room room = level.Rooms[2];
            Assert.AreEqual(1, room.Lights.FindAll(l => l is TR5FogBulb).Count);
            Assert.AreNotEqual(1, room.Lights.Count);

            TR5FogBulb bulb = room.Lights.Find(l => l is TR5FogBulb) as TR5FogBulb;
            Assert.AreEqual(46592, bulb.Position.X);
            Assert.AreEqual(-1792, bulb.Position.Y);
            Assert.AreEqual(38400, bulb.Position.Z);
            Assert.AreEqual((byte)255, (byte)(bulb.Colour.R * 255));
            Assert.AreEqual((byte)191, (byte)(bulb.Colour.G * 255));
            Assert.AreEqual((byte)127, (byte)(bulb.Colour.B * 255));
            Assert.AreEqual(800, bulb.Density);
            Assert.AreEqual(832, bulb.Radius);
        }

        // Initial read test
        Test();

        // Ensure the same after write
        level = WriteReadTempLevel(level);
        Test();
    }

    [TestMethod]
    [Description("Test fog bulbs in a room that has no other lights.")]
    public void TestFogBulbsNoLights()
    {
        TR5Level level = GetTR5Level(TR5LevelNames.ROME);

        void Test()
        {
            TR5Room room = level.Rooms[26];
            Assert.AreEqual(1, room.Lights.Count);
            Assert.IsTrue(room.Lights[0] is TR5FogBulb);

            TR5FogBulb bulb = room.Lights[0] as TR5FogBulb;
            Assert.AreEqual(30208, bulb.Position.X);
            Assert.AreEqual(-2048, bulb.Position.Y);
            Assert.AreEqual(20992, bulb.Position.Z);
            Assert.AreEqual((byte)191, (byte)(bulb.Colour.R * 255));
            Assert.AreEqual((byte)143, (byte)(bulb.Colour.G * 255));
            Assert.AreEqual((byte)207, (byte)(bulb.Colour.B * 255));
            Assert.AreEqual(736, bulb.Density);
            Assert.AreEqual(1600, bulb.Radius);
        }

        // Initial read test
        Test();

        // Ensure the same after write
        level = WriteReadTempLevel(level);
        Test();
    }

    [TestMethod]
    [Description("Test adding a new non-fog light.")]
    public void AddNonFogLight()
    {
        TR5Level level = GetTR5Level(TR5LevelNames.ROME);

        TR5Room room = level.Rooms[0];
        Assert.AreEqual(2, room.Lights.Count);
        Assert.IsFalse(room.Lights[0] is TR5FogBulb);
        Assert.IsFalse(room.Lights[1] is TR5FogBulb);

        TR5PointLight light = new()
        {
            Colour = new()
            {
                R = 0.5f,
                G = 0.8f,
                B = 0.2f
            },
            Inner = 256,
            Outer = 1024,
            Position = new()
            {
                X = 47575,
                Y = -512,
                Z = 31178
            }
        };
        level.Rooms[0].Lights.Add(light);

        level = WriteReadTempLevel(level);
        room = level.Rooms[0];
        Assert.AreEqual(3, room.Lights.Count);
        Assert.IsFalse(room.Lights[0] is TR5FogBulb);
        Assert.IsFalse(room.Lights[1] is TR5FogBulb);
        Assert.IsTrue(room.Lights[2] is TR5PointLight);

        TR5PointLight newLight = room.Lights[2] as TR5PointLight;
        Assert.AreEqual(light.Colour.R, newLight.Colour.R);
        Assert.AreEqual(light.Colour.G, newLight.Colour.G);
        Assert.AreEqual(light.Colour.B, newLight.Colour.B);
        Assert.AreEqual(light.Inner, newLight.Inner);
        Assert.AreEqual(light.Outer, newLight.Outer);
        Assert.AreEqual(light.Position.X, newLight.Position.X);
        Assert.AreEqual(light.Position.Y, newLight.Position.Y);
        Assert.AreEqual(light.Position.Z, newLight.Position.Z);
    }

    [TestMethod]
    [Description("Test adding a new fog light.")]
    public void AddFogLight()
    {
        TR5Level level = GetTR5Level(TR5LevelNames.ROME);

        TR5Room room = level.Rooms[0];
        Assert.AreEqual(2, room.Lights.Count);
        Assert.IsFalse(room.Lights[0] is TR5FogBulb);
        Assert.IsFalse(room.Lights[1] is TR5FogBulb);

        TR5FogBulb bulb = new()
        {
            Colour = new()
            {
                R = 0.5f,
                G = 0.8f,
                B = 0.2f
            },
            Density = 607,
            Radius = 342,
            Position = new()
            {
                X = 47575,
                Y = -512,
                Z = 31178
            }
        };
        level.Rooms[0].Lights.Add(bulb);

        level = WriteReadTempLevel(level);
        room = level.Rooms[0];
        Assert.AreEqual(3, room.Lights.Count);
        Assert.IsFalse(room.Lights[0] is TR5FogBulb);
        Assert.IsFalse(room.Lights[1] is TR5FogBulb);
        Assert.IsTrue(room.Lights[2] is TR5FogBulb);

        TR5FogBulb newBulb = room.Lights[2] as TR5FogBulb;
        Assert.AreEqual(bulb.Colour.R, newBulb.Colour.R);
        Assert.AreEqual(bulb.Colour.G, newBulb.Colour.G);
        Assert.AreEqual(bulb.Colour.B, newBulb.Colour.B);
        Assert.AreEqual(bulb.Density, newBulb.Density);
        Assert.AreEqual(bulb.Radius, newBulb.Radius);
        Assert.AreEqual(bulb.Position.X, newBulb.Position.X);
        Assert.AreEqual(bulb.Position.Y, newBulb.Position.Y);
        Assert.AreEqual(bulb.Position.Z, newBulb.Position.Z);
    }

    [TestMethod]
    [Description("Test removing a non-fog light.")]
    public void RemoveNonFogLight()
    {
        TR5Level level = GetTR5Level(TR5LevelNames.ROME);

        TR5Room room = level.Rooms[34];
        Assert.AreEqual(2, room.Lights.Count);
        Assert.IsTrue(room.Lights[0] is TR5ShadowLight);
        Assert.IsTrue(room.Lights[1] is TR5FogBulb);

        room.Lights.RemoveAt(0);

        level = WriteReadTempLevel(level);
        room = level.Rooms[34];

        Assert.AreEqual(1, room.Lights.Count);
        Assert.IsTrue(room.Lights[0] is TR5FogBulb);
    }

    [TestMethod]
    [Description("Test removing a fog light.")]
    public void RemoveFogLight()
    {
        TR5Level level = GetTR5Level(TR5LevelNames.ROME);

        TR5Room room = level.Rooms[34];
        Assert.AreEqual(2, room.Lights.Count);
        Assert.IsTrue(room.Lights[0] is TR5ShadowLight);
        Assert.IsTrue(room.Lights[1] is TR5FogBulb);

        room.Lights.RemoveAt(1);

        level = WriteReadTempLevel(level);
        room = level.Rooms[34];

        Assert.AreEqual(1, room.Lights.Count);
        Assert.IsTrue(room.Lights[0] is TR5ShadowLight);
    }
}
