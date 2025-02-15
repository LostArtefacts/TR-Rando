using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRLevelControlTests.TR5;

[TestClass]
[TestCategory("OriginalIO")]
public class RoomTests : TestBase
{
    public static IEnumerable<object[]> GetAllLevels() => GetLevelNames(TR5LevelNames.AsList);

    private static readonly Dictionary<string, int> _roomCounts = new()
    {
        [TR5LevelNames.ROME] = 242,
        [TR5LevelNames.MARKETS] = 252,
        [TR5LevelNames.COLOSSEUM] = 130,
        [TR5LevelNames.BASE] = 169,
        [TR5LevelNames.SUBMARINE] = 195,
        [TR5LevelNames.DEEPSEA] = 137,
        [TR5LevelNames.SINKING] = 191,
        [TR5LevelNames.GALLOWS] = 243,
        [TR5LevelNames.LABYRINTH] = 254,
        [TR5LevelNames.MILL] = 211,
        [TR5LevelNames.FLOOR13] = 165,
        [TR5LevelNames.ESCAPE] = 179,
        [TR5LevelNames.REDALERT] = 218,
    };

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    [Description("Compare each room in each level before and after writing to ensure identical properties.")]
    public void TestRoomIOClassic(string levelName)
    {
        TR5Level level = GetTR5Level(levelName, false);
        TestRoomIO(level, _roomCounts[levelName]);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestRoomIORemastered(string levelName)
    {
        TR5Level level = GetTR5Level(levelName, true);
        TestRoomIO(level, _roomCounts[levelName]);
    }

    [TestMethod]
    [Description("Add a room to test roomlet expansion and squashing.")]
    [DataRow(true)]
    [DataRow(false)]
    public void TestRoomToRoomlet(bool remastered)
    {
        TR5Level level = GetTR5Level(TR5LevelNames.ROME, remastered);
        TR5Room room = new()
        {
            AlternateGroup = 2,
            AlternateRoom = 3,
            Colour = new()
            {
                Red = 120,
                Green = 80,
                Blue = 91
            },
            Flags = TRRoomFlag.Skybox,
            Info = new()
            {
                X = 1024,
                YBottom = 256,
                YTop = -2048,
                Z = 4096
            },
            Lights = new(),
            Mesh = new()
            {
                Rectangles = new(),
                Triangles = new(),
                Vertices = new()
            },
            NumXSectors = 10,
            NumZSectors = 5,
            Portals = new()
            {
                new()
                {
                    AdjoiningRoom = 1,
                    Normal = new() { X = 1 },
                    Vertices = new()
                    {
                        new()
                        {
                            X = 1024,
                            Y = -768,
                            Z = 1024
                        },
                        new()
                        {
                            X = 1024,
                            Y = -768,
                            Z = 2048
                        },
                        new()
                        {
                            X = 1024,
                            Y = 256,
                            Z = 2048
                        },
                        new()
                        {
                            X = 1024,
                            Y = 256,
                            Z = 1024
                        }
                    }
                }
            },
            ReverbMode = TRPSXReverbMode.LargeRoom,
            Sectors = new(),
            StaticMeshes = new(),
            WaterScheme = 2
        };
        level.Rooms.Add(room);

        Random rand = new();
        for (int i = 0; i < 50; i++)
        {
            room.Sectors.Add(new()
            {
                BoxIndex = (ushort)i,
                Ceiling = (sbyte)i,
                Floor = (sbyte)-i,
                RoomAbove = (byte)rand.Next(0, 256),
                RoomBelow = (byte)rand.Next(0, 256),
            });
        }

        for (int i = 0; i < 100; i++)
        {
            room.Mesh.Vertices.Add(new()
            {
                Colour = new()
                {
                    Red = (byte)rand.Next(0, 255),
                    Green = (byte)rand.Next(0, 255),
                    Blue = (byte)rand.Next(0, 255),
                },
                Normal = new()
                {
                    X = rand.Next(0, 3) - 1,
                    Y = rand.Next(0, 3) - 1,
                    Z = rand.Next(0, 3) - 1,
                },
                Vertex = new()
                {
                    X = (short)(TRConsts.Step4 * rand.Next(1, 10)),
                    Y = (short)(TRConsts.Step1 * rand.Next(1, 10)),
                    Z = (short)(TRConsts.Step4 * rand.Next(1, 10)),
                }
            });
        }

        for (int i = 0; i < 60; i++)
        {
            room.Mesh.Rectangles.Add(new()
            {
                Type = TRFaceType.Rectangle,
                Texture = (ushort)rand.Next(0, 1000),
                Vertices = new()
                {
                    (ushort)rand.Next(0, 100),
                    (ushort)rand.Next(0, 100),
                    (ushort)rand.Next(0, 100),
                    (ushort)rand.Next(0, 100),
                }
            });
        }

        for (int i = 0; i < 40; i++)
        {
            room.Mesh.Triangles.Add(new()
            {
                Type = TRFaceType.Triangle,
                Texture = (ushort)rand.Next(0, 1000),
                Vertices = new()
                {
                    (ushort)rand.Next(0, 100),
                    (ushort)rand.Next(0, 100),
                    (ushort)rand.Next(0, 100),
                }
            });
        }

        level = WriteReadTempLevel(level);
        TR5Room newRoom = level.Rooms[^1];

        CompareRooms(room, newRoom);
    }

    private static void TestRoomIO(TR5Level level, int expectedRoomCount)
    {
        Assert.AreEqual(expectedRoomCount, level.Rooms.Count);
        List<TR5Room> rooms = new(level.Rooms);

        level = WriteReadTempLevel(level);
        Assert.AreEqual(expectedRoomCount, level.Rooms.Count);

        for (int i = 0; i < rooms.Count; i++)
        {
            CompareRooms(rooms[i], level.Rooms[i]);
        }
    }

    private static void CompareRooms(TR5Room oldRoom, TR5Room newRoom)
    {
        Assert.AreNotEqual(oldRoom, newRoom);

        Assert.AreEqual(oldRoom.AlternateGroup, newRoom.AlternateGroup);
        Assert.AreEqual(oldRoom.AlternateRoom, newRoom.AlternateRoom);
        Assert.AreEqual(oldRoom.Colour.Red, newRoom.Colour.Red);
        Assert.AreEqual(oldRoom.Colour.Green, newRoom.Colour.Green);
        Assert.AreEqual(oldRoom.Colour.Blue, newRoom.Colour.Blue);
        Assert.AreEqual(oldRoom.Flags, newRoom.Flags);
        Assert.AreEqual(oldRoom.Info.X, newRoom.Info.X);
        Assert.AreEqual(oldRoom.Info.YBottom, newRoom.Info.YBottom);
        Assert.AreEqual(oldRoom.Info.YTop, newRoom.Info.YTop);
        Assert.AreEqual(oldRoom.Info.Z, newRoom.Info.Z);
        Assert.AreEqual(oldRoom.NumXSectors, newRoom.NumXSectors);
        Assert.AreEqual(oldRoom.NumZSectors, newRoom.NumZSectors);
        Assert.AreEqual(oldRoom.ReverbMode, newRoom.ReverbMode);
        Assert.AreEqual(oldRoom.WaterScheme, newRoom.WaterScheme);

        Assert.AreEqual(oldRoom.Lights.Count, newRoom.Lights.Count);
        for (int i = 0; i < oldRoom.Lights.Count; i++)
        {
            TR5RoomLight l0 = oldRoom.Lights[i];
            TR5RoomLight l1 = newRoom.Lights[i];
            Assert.AreEqual(l0.Type, l1.Type);
            Assert.AreEqual(l0.Position.X, l1.Position.X);
            Assert.AreEqual(l0.Position.Y, l1.Position.Y);
            Assert.AreEqual(l0.Position.Z, l1.Position.Z);
            Assert.AreEqual(l0.Colour.R, l1.Colour.R);
            Assert.AreEqual(l0.Colour.G, l1.Colour.G);
            Assert.AreEqual(l0.Colour.B, l1.Colour.B);

            if (l0 is TR5GeneralLight)
            {
                TR5GeneralLight g0 = l0 as TR5GeneralLight;
                TR5GeneralLight g1 = l1 as TR5GeneralLight;
                Assert.AreEqual(g0.Inner, g1.Inner);
                Assert.AreEqual(g0.Outer, g1.Outer);
                Assert.AreEqual(g0.InnerAngle, g1.InnerAngle);
                Assert.AreEqual(g0.OuterAngle, g1.OuterAngle);
                Assert.AreEqual(g0.Range, g1.Range);
                Assert.AreEqual(g0.Direction, g1.Direction);
            }
            else
            {
                TR5FogBulb f0 = l0 as TR5FogBulb;
                TR5FogBulb f1 = l1 as TR5FogBulb;
                Assert.AreEqual(f0.Radius, f1.Radius);
                Assert.AreEqual(f0.Density, f1.Density);
            }
        }

        Assert.AreEqual(oldRoom.Portals.Count, newRoom.Portals.Count);
        for (int i = 0; i < oldRoom.Portals.Count; i++)
        {
            TRRoomPortal p0 = oldRoom.Portals[i];
            TRRoomPortal p1 = newRoom.Portals[i];
            Assert.AreEqual(p0.AdjoiningRoom, p1.AdjoiningRoom);
            Assert.AreEqual(p0.Normal.X, p1.Normal.X);
            Assert.AreEqual(p0.Normal.Y, p1.Normal.Y);
            Assert.AreEqual(p0.Normal.Z, p1.Normal.Z);

            Assert.AreEqual(p0.Vertices.Count, p1.Vertices.Count);
            for (int j = 0; j < p0.Vertices.Count; j++)
            {
                TRVertex v0 = p0.Vertices[j];
                TRVertex v1 = p1.Vertices[j];
                Assert.AreEqual(v0.X, v1.X);
                Assert.AreEqual(v0.Y, v1.Y);
                Assert.AreEqual(v0.Z, v1.Z);
            }
        }

        Assert.AreEqual(oldRoom.Sectors.Count, newRoom.Sectors.Count);
        for (int i = 0; i < oldRoom.Sectors.Count; i++)
        {
            TRRoomSector s0 = oldRoom.Sectors[i];
            TRRoomSector s1 = newRoom.Sectors[i];
            Assert.AreEqual(s0.BoxIndex, s1.BoxIndex);
            Assert.AreEqual(s0.Ceiling, s1.Ceiling);
            Assert.AreEqual(s0.FDIndex, s1.FDIndex);
            Assert.AreEqual(s0.Floor, s1.Floor);
            Assert.AreEqual(s0.RoomAbove, s1.RoomAbove);
            Assert.AreEqual(s0.RoomBelow, s1.RoomBelow);
        }

        Assert.AreEqual(oldRoom.Mesh.Vertices.Count, newRoom.Mesh.Vertices.Count);
        for (int i = 0; i < oldRoom.Mesh.Vertices.Count; i++)
        {
            TR5RoomVertex v0 = oldRoom.Mesh.Vertices[i];
            TR5RoomVertex v1 = newRoom.Mesh.Vertices[i];
            Assert.AreEqual(v0.Colour.Red, v1.Colour.Red);
            Assert.AreEqual(v0.Colour.Green, v1.Colour.Green);
            Assert.AreEqual(v0.Colour.Blue, v1.Colour.Blue);
            Assert.AreEqual(v0.Normal.X, v1.Normal.X);
            Assert.AreEqual(v0.Normal.Y, v1.Normal.Y);
            Assert.AreEqual(v0.Normal.Z, v1.Normal.Z);
            Assert.AreEqual(v0.Vertex.X, v1.Vertex.X);
            Assert.AreEqual(v0.Vertex.Y, v1.Vertex.Y);
            Assert.AreEqual(v0.Vertex.Z, v1.Vertex.Z);
        }

        Assert.AreEqual(oldRoom.Mesh.Rectangles.Count, newRoom.Mesh.Rectangles.Count);
        for (int i = 0; i < oldRoom.Mesh.Rectangles.Count; i++)
        {
            TRFace f0 = oldRoom.Mesh.Rectangles[i];
            TRFace f1 = newRoom.Mesh.Rectangles[i];
            Assert.AreEqual(f0.Texture, f1.Texture);
            Assert.AreEqual(f0.DoubleSided, f1.DoubleSided);
            CollectionAssert.AreEqual(f0.Vertices, f1.Vertices);
        }

        Assert.AreEqual(oldRoom.Mesh.Triangles.Count, newRoom.Mesh.Triangles.Count);
        for (int i = 0; i < oldRoom.Mesh.Triangles.Count; i++)
        {
            TRFace f0 = oldRoom.Mesh.Triangles[i];
            TRFace f1 = newRoom.Mesh.Triangles[i];
            Assert.AreEqual(f0.Texture, f1.Texture);
            Assert.AreEqual(f0.DoubleSided, f1.DoubleSided);
            CollectionAssert.AreEqual(f0.Vertices, f1.Vertices);
        }
    }
}
