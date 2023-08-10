using Microsoft.VisualStudio.TestTools.UnitTesting;
using TRFDControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Base.Enums;
using TRLevelControl.Model.Enums;
using TRRandomizerCore.Secrets;

namespace TRLevelControlTests.TR3;

[TestClass]
[TestCategory("OriginalIO")]
public class IOTests : TestBase
{
    [TestMethod]
    [DataRow(TR3LevelNames.ASSAULT)]
    [DataRow(TR3LevelNames.JUNGLE)]
    [DataRow(TR3LevelNames.JUNGLE_CUT)]
    [DataRow(TR3LevelNames.RUINS)]
    [DataRow(TR3LevelNames.RUINS_CUT)]
    [DataRow(TR3LevelNames.GANGES)]
    [DataRow(TR3LevelNames.CAVES)]
    [DataRow(TR3LevelNames.COASTAL)]
    [DataRow(TR3LevelNames.COASTAL_CUT)]
    [DataRow(TR3LevelNames.CRASH)]
    [DataRow(TR3LevelNames.CRASH_CUT)]
    [DataRow(TR3LevelNames.MADUBU)]
    [DataRow(TR3LevelNames.PUNA)]
    [DataRow(TR3LevelNames.THAMES)]
    [DataRow(TR3LevelNames.THAMES_CUT)]
    [DataRow(TR3LevelNames.ALDWYCH)]
    [DataRow(TR3LevelNames.ALDWYCH_CUT)]
    [DataRow(TR3LevelNames.LUDS)]
    [DataRow(TR3LevelNames.LUDS_CUT)]
    [DataRow(TR3LevelNames.CITY)]
    [DataRow(TR3LevelNames.HALLOWS)]
    [DataRow(TR3LevelNames.NEVADA)]
    [DataRow(TR3LevelNames.NEVADA_CUT)]
    [DataRow(TR3LevelNames.HSC)]
    [DataRow(TR3LevelNames.HSC_CUT)]
    [DataRow(TR3LevelNames.AREA51)]
    [DataRow(TR3LevelNames.ANTARC)]
    [DataRow(TR3LevelNames.ANTARC_CUT)]
    [DataRow(TR3LevelNames.RXTECH)]
    [DataRow(TR3LevelNames.TINNOS)]
    [DataRow(TR3LevelNames.TINNOS_CUT)]
    [DataRow(TR3LevelNames.WILLIE)]
    [DataRow(TR3LevelNames.FLING)]
    [DataRow(TR3LevelNames.LAIR)]
    [DataRow(TR3LevelNames.CLIFF)]
    [DataRow(TR3LevelNames.FISHES)]
    [DataRow(TR3LevelNames.MADHOUSE)]
    [DataRow(TR3LevelNames.REUNION)]
    public void TestReadWrite(string levelName)
    {
        ReadWriteLevel(levelName, TRGameVersion.TR3);
    }

    [TestMethod]
    public void Floordata_ReadWrite_DefaultTest()
    {
        TR3Level lvl = GetTR3Level(TR3LevelNames.RXTECH);

        //Store the original floordata from the level
        ushort[] originalFData = new ushort[lvl.NumFloorData];
        Array.Copy(lvl.FloorData, originalFData, lvl.NumFloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new();
        fdataReader.ParseFromLevel(lvl);
        fdataReader.WriteToLevel(lvl);

        //Store the new floordata written back by FDControl
        ushort[] newFData = lvl.FloorData;

        //Compare to make sure the original fdata was written back.
        CollectionAssert.AreEqual(originalFData, newFData, "Floordata does not match");
        Assert.AreEqual((uint)newFData.Length, lvl.NumFloorData);
    }

    [TestMethod]
    public void Floordata_ReadWrite_LevelHasMonkeySwingTest()
    {
        TR3Level lvl = GetTR3Level(TR3LevelNames.THAMES);

        //Store the original floordata from the level
        ushort[] originalFData = new ushort[lvl.NumFloorData];
        Array.Copy(lvl.FloorData, originalFData, lvl.NumFloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new();
        fdataReader.ParseFromLevel(lvl);
        fdataReader.WriteToLevel(lvl);

        //Store the new floordata written back by FDControl
        ushort[] newFData = lvl.FloorData;

        //Compare to make sure the original fdata was written back.
        CollectionAssert.AreEqual(originalFData, newFData, "Floordata does not match");
        Assert.AreEqual((uint)newFData.Length, lvl.NumFloorData);
    }

    [TestMethod]
    public void ModifyZonesTest()
    {
        TR3Level lvl = GetTR3Level(TR3LevelNames.JUNGLE);

        // For every box, store the current zone. We use the serialized form
        // for comparison.
        Dictionary<int, byte[]> flipOffZones = new();
        Dictionary<int, byte[]> flipOnZones = new();
        for (int i = 0; i < lvl.NumBoxes; i++)
        {
            flipOffZones[i] = lvl.Zones[i][FlipStatus.Off].Serialize();
            flipOnZones[i] = lvl.Zones[i][FlipStatus.On].Serialize();
        }

        // Add a new box
        List<TR2Box> boxes = lvl.Boxes.ToList();
        boxes.Add(boxes[0]);
        lvl.Boxes = boxes.ToArray();
        lvl.NumBoxes++;

        // Add a new zone for the box and store its serialized form for comparison
        int newBoxIndex = (int)(lvl.NumBoxes - 1);
        TR2BoxUtilities.DuplicateZone(lvl, 0);
        flipOffZones[newBoxIndex] = lvl.Zones[newBoxIndex][FlipStatus.Off].Serialize();
        flipOnZones[newBoxIndex] = lvl.Zones[newBoxIndex][FlipStatus.On].Serialize();

        // Verify the number of zone ushorts matches what's expected for the box count
        Assert.AreEqual(TR2BoxUtilities.FlattenZones(lvl.Zones).Length, (int)(10 * lvl.NumBoxes));

        // Write and re-read the level
        lvl = WriteReadTempLevel(lvl);

        // Capture all of the zones again. Make sure the addition of the zone above didn't
        // affect any of the others and that the addition itself matches after IO.
        for (int i = 0; i < lvl.NumBoxes; i++)
        {
            byte[] flipOff = lvl.Zones[i][FlipStatus.Off].Serialize();
            Assert.IsTrue(flipOffZones.ContainsKey(i));
            CollectionAssert.AreEqual(flipOffZones[i], flipOff);

            byte[] flipOn = lvl.Zones[i][FlipStatus.On].Serialize();
            Assert.IsTrue(flipOnZones.ContainsKey(i));
            CollectionAssert.AreEqual(flipOnZones[i], flipOn);
        }
    }

    [TestMethod]
    public void ModifyOverlapsTest()
    {
        TR3Level lvl = GetTR3Level(TR3LevelNames.JUNGLE);

        // For every box, store the current list of overlaps and the overlap starting
        // index itself (which also stores Blockable/Blocked bits).
        Dictionary<int, List<ushort>> boxOverlaps = new();
        Dictionary<int, short> boxOverlapIndices = new();
        for (int i = 0; i < lvl.NumBoxes; i++)
        {
            boxOverlaps[i] = TR2BoxUtilities.GetOverlaps(lvl, lvl.Boxes[i]);
            boxOverlapIndices[i] = lvl.Boxes[i].OverlapIndex;
        }

        // TR3 allows different boxes to point to the same overlap index.
        // For testing we just write everything back and repeat the collection
        // process above to ensure we get the same results.

        // Write everything back with no changes.
        for (int i = 0; i < lvl.NumBoxes; i++)
        {
            TR2BoxUtilities.UpdateOverlaps(lvl, lvl.Boxes[i], boxOverlaps[i]);
        }

        Dictionary<int, List<ushort>> newBoxOverlaps = new();
        for (int i = 0; i < lvl.NumBoxes; i++)
        {
            newBoxOverlaps[i] = TR2BoxUtilities.GetOverlaps(lvl, lvl.Boxes[i]);
        }

        Assert.AreEqual(boxOverlaps.Count, newBoxOverlaps.Count);
        foreach (int boxIndex in boxOverlaps.Keys)
        {
            CollectionAssert.AreEqual(boxOverlaps[boxIndex], newBoxOverlaps[boxIndex]);
        }

        // Add a new overlap to the first box, selecting a box that isn't already there.
        for (ushort i = 1; i < lvl.NumBoxes; i++)
        {
            if (!boxOverlaps[0].Contains(i))
            {
                boxOverlaps[0].Add(i);
                break;
            }
        }

        // Write the overlap list back to the level for box 0.
        TR2BoxUtilities.UpdateOverlaps(lvl, lvl.Boxes[0], boxOverlaps[0]);

        // Write and re-read the level
        lvl = WriteReadTempLevel(lvl);

        // Capture all of the overlaps again and confirm the numbers are what we expect i.e.
        // the new overlap for box 0 exists and none of the other overlaps were affected by
        // the addition.
        for (int i = 0; i < lvl.NumBoxes; i++)
        {
            List<ushort> overlaps = TR2BoxUtilities.GetOverlaps(lvl, lvl.Boxes[i]);
            Assert.IsTrue(boxOverlaps.ContainsKey(i));
            CollectionAssert.AreEqual(boxOverlaps[i], overlaps);
        }
    }

    [TestMethod]
    public void TestSecretTriggerMasks()
    {
        // TR3 has a max of 6 secrets in any level but we'll test a silly number to ensure the algorithm works.
        for (int totalSecrets = 1; totalSecrets <= 21; totalSecrets++)
        {
            // The number of doors determines the trigger masks
            int requiredDoors = (int)Math.Ceiling((double)totalSecrets / TRSecretPlacement<TR3Entities>.MaskBits);
            List<int> doors = new(requiredDoors);
            for (int i = 0; i < requiredDoors; i++)
            {
                doors.Add(i);
            }

            List<TRSecretPlacement<TR3Entities>> secrets = new();

            // Create a secret up to the limit for this "level" and set its mask and door
            for (ushort secretIndex = 0; secretIndex < totalSecrets; secretIndex++)
            {
                TRSecretPlacement<TR3Entities> secret = new()
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
                List<TRSecretPlacement<TR3Entities>> doorTriggers = secrets.FindAll(s => s.DoorIndex == i);
                int mask = 0;
                doorTriggers.ForEach(s => mask += s.TriggerMask);

                Assert.AreEqual(TRSecretPlacement<TR3Entities>.FullActivation, mask);
            }
        }
    }
}
