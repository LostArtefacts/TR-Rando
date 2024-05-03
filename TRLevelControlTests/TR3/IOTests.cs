using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Secrets;

namespace TRLevelControlTests.TR3;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    public static IEnumerable<object[]> GetAllLevels() => GetLevelNames(TR3LevelNames.AsOrderedList);

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR3);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestAgressiveFloorData(string levelName)
    {
        TR3Level level = GetTR3Level(levelName);
        IEnumerable<TRRoomSector> allFDSectors = level.Rooms.SelectMany(r => r.Sectors.Where(s => s.FDIndex != 0));

        foreach (TRRoomSector sector in allFDSectors)
        {
            Assert.IsTrue(level.FloorData.ContainsKey(sector.FDIndex));
        }
        Assert.AreEqual(allFDSectors.Count(), allFDSectors.DistinctBy(s => s.FDIndex).Count());
    }

    //[TestMethod]
    //public void Floordata_ReadWrite_LevelHasMonkeySwingTest()
    //{
    //    TR3Level lvl = GetTR3Level(TR3LevelNames.THAMES);

    //    //Store the original floordata from the level
    //    List<ushort> originalFData = new(lvl.FloorData);

    //    //Parse the floordata using FDControl and re-write the parsed data back
    //    FDControl fdataReader = new();
    //    fdataReader.ParseFromLevel(lvl);
    //    fdataReader.WriteToLevel(lvl);

    //    //Compare to make sure the original fdata was written back.
    //    CollectionAssert.AreEqual(originalFData, lvl.FloorData, "Floordata does not match");;
    //}

    [TestMethod]
    public void ModifyZonesTest()
    {
        //TR3Level lvl = GetTR3Level(TR3LevelNames.JUNGLE);

        //// For every box, store the current zone. We use the serialized form
        //// for comparison.
        //Dictionary<int, byte[]> flipOffZones = new();
        //Dictionary<int, byte[]> flipOnZones = new();
        //for (int i = 0; i < lvl.Boxes.Count; i++)
        //{
        //    flipOffZones[i] = lvl.Zones[i][FlipStatus.Off].Serialize();
        //    flipOnZones[i] = lvl.Zones[i][FlipStatus.On].Serialize();
        //}

        //// Add a new box
        //lvl.Boxes.Add(lvl.Boxes[0]);

        //// Add a new zone for the box and store its serialized form for comparison
        //int newBoxIndex = (int)(lvl.Boxes.Count - 1);
        //TR2BoxUtilities.DuplicateZone(lvl, 0);
        //flipOffZones[newBoxIndex] = lvl.Zones[newBoxIndex][FlipStatus.Off].Serialize();
        //flipOnZones[newBoxIndex] = lvl.Zones[newBoxIndex][FlipStatus.On].Serialize();

        //// Verify the number of zone ushorts matches what's expected for the box count
        //Assert.AreEqual(TR2BoxUtilities.FlattenZones(lvl.Zones).Count, 10 * lvl.Boxes.Count);

        //// Write and re-read the level
        //lvl = WriteReadTempLevel(lvl);

        //// Capture all of the zones again. Make sure the addition of the zone above didn't
        //// affect any of the others and that the addition itself matches after IO.
        //for (int i = 0; i < lvl.Boxes.Count; i++)
        //{
        //    byte[] flipOff = lvl.Zones[i][FlipStatus.Off].Serialize();
        //    Assert.IsTrue(flipOffZones.ContainsKey(i));
        //    CollectionAssert.AreEqual(flipOffZones[i], flipOff);

        //    byte[] flipOn = lvl.Zones[i][FlipStatus.On].Serialize();
        //    Assert.IsTrue(flipOnZones.ContainsKey(i));
        //    CollectionAssert.AreEqual(flipOnZones[i], flipOn);
        //}
    }

    [TestMethod]
    public void TestSecretTriggerMasks()
    {
        // TR3 has a max of 6 secrets in any level but we'll test a silly number to ensure the algorithm works.
        for (int totalSecrets = 1; totalSecrets <= 21; totalSecrets++)
        {
            // The number of doors determines the trigger masks
            int requiredDoors = (int)Math.Ceiling((double)totalSecrets / TRConsts.MaskBits);
            List<int> doors = new(requiredDoors);
            for (int i = 0; i < requiredDoors; i++)
            {
                doors.Add(i);
            }

            List<TRSecretPlacement<TR3Type>> secrets = new();

            // Create a secret up to the limit for this "level" and set its mask and door
            for (short secretIndex = 0; secretIndex < totalSecrets; secretIndex++)
            {
                TRSecretPlacement<TR3Type> secret = new()
                {
                    SecretIndex = secretIndex
                };
                secret.SetMaskAndDoor(totalSecrets, doors);
                secrets.Add(secret);
            }

            // Now test that for each door, the sum of the trigger masks for the secrets
            // allocated to it equals full activation i.e. 31.
            for (int i = 0; i < requiredDoors; i++)
            {
                List<TRSecretPlacement<TR3Type>> doorTriggers = secrets.FindAll(s => s.DoorIndex == i);
                int mask = 0;
                doorTriggers.ForEach(s => mask += s.TriggerMask);

                Assert.AreEqual(TRConsts.FullMask, mask);
            }
        }
    }
}
