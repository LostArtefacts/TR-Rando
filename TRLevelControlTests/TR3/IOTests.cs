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
    public void TestOGReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR3, false);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestRemasteredReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR3, true);
    }

    [TestMethod]
    [DynamicData(nameof(GetAllLevels), DynamicDataSourceType.Method)]
    public void TestPDPReadWrite(string levelName)
    {
        ReadWritePDP(levelName, TRGameVersion.TR3);
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
