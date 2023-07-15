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
    public void ReadWriteGym()
    {
        ReadWriteTR3Level(TR3LevelNames.ASSAULT);
    }

    [TestMethod]

    public void ReadWriteJungle()
    {
        ReadWriteTR3Level(TR3LevelNames.JUNGLE);
    }

    [TestMethod]
    public void ReadWriteCut1()
    {
        ReadWriteTR3Level(TR3LevelNames.JUNGLE_CUT);
    }

    [TestMethod]
    public void ReadWriteTemple()
    {
        ReadWriteTR3Level(TR3LevelNames.RUINS);
    }

    [TestMethod]
    public void ReadWriteCut2()
    {
        ReadWriteTR3Level(TR3LevelNames.RUINS_CUT);
    }

    [TestMethod]
    public void ReadWriteGanges()
    {
        ReadWriteTR3Level(TR3LevelNames.GANGES);
    }

    [TestMethod]
    public void ReadWriteKaliya()
    {
        ReadWriteTR3Level(TR3LevelNames.CAVES);
    }

    [TestMethod]
    public void ReadWriteCoastal()
    {
        ReadWriteTR3Level(TR3LevelNames.COASTAL);
    }

    [TestMethod]
    public void ReadWriteCut3()
    {
        ReadWriteTR3Level(TR3LevelNames.COASTAL_CUT);
    }

    [TestMethod]
    public void ReadWriteCrash()
    {
        ReadWriteTR3Level(TR3LevelNames.CRASH);
    }

    [TestMethod]
    public void ReadWriteCut4()
    {
        ReadWriteTR3Level(TR3LevelNames.CRASH_CUT);
    }

    [TestMethod]
    public void ReadWriteMadubu()
    {
        ReadWriteTR3Level(TR3LevelNames.MADUBU);
    }

    [TestMethod]
    public void ReadWritePuna()
    {
        ReadWriteTR3Level(TR3LevelNames.PUNA);
    }

    [TestMethod]
    public void ReadWriteThames()
    {
        ReadWriteTR3Level(TR3LevelNames.THAMES);
    }

    [TestMethod]
    public void ReadWriteCut5()
    {
        ReadWriteTR3Level(TR3LevelNames.THAMES_CUT);
    }

    [TestMethod]
    public void ReadWriteAldwych()
    {
        ReadWriteTR3Level(TR3LevelNames.ALDWYCH);
    }

    [TestMethod]
    public void ReadWriteCut6()
    {
        ReadWriteTR3Level(TR3LevelNames.ALDWYCH_CUT);
    }

    [TestMethod]
    public void ReadWriteLuds()
    {
        ReadWriteTR3Level(TR3LevelNames.LUDS);
    }

    [TestMethod]
    public void ReadWriteCut7()
    {
        ReadWriteTR3Level(TR3LevelNames.LUDS_CUT);
    }

    [TestMethod]
    public void ReadWriteCity()
    {
        ReadWriteTR3Level(TR3LevelNames.CITY);
    }

    [TestMethod]
    public void ReadWriteNevada()
    {
        ReadWriteTR3Level(TR3LevelNames.NEVADA);
    }

    [TestMethod]
    public void ReadWriteCut8()
    {
        ReadWriteTR3Level(TR3LevelNames.NEVADA_CUT);
    }

    [TestMethod]
    public void ReadWriteHSC()
    {
        ReadWriteTR3Level(TR3LevelNames.HSC);
    }

    [TestMethod]
    public void ReadWriteCut9()
    {
        ReadWriteTR3Level(TR3LevelNames.HSC_CUT);
    }

    [TestMethod]
    public void ReadWriteArea51()
    {
        ReadWriteTR3Level(TR3LevelNames.AREA51);
    }

    [TestMethod]
    public void ReadWriteAntarctica()
    {
        ReadWriteTR3Level(TR3LevelNames.ANTARC);
    }

    [TestMethod]
    public void ReadWriteCut10()
    {
        ReadWriteTR3Level(TR3LevelNames.ANTARC_CUT);
    }

    [TestMethod]
    public void ReadWriteMines()
    {
        ReadWriteTR3Level(TR3LevelNames.RXTECH);
    }

    [TestMethod]
    public void ReadWriteTinnos()
    {
        ReadWriteTR3Level(TR3LevelNames.TINNOS);
    }

    [TestMethod]
    public void ReadWriteCut11()
    {
        ReadWriteTR3Level(TR3LevelNames.TINNOS_CUT);
    }

    [TestMethod]
    public void ReadWriteWillie()
    {
        ReadWriteTR3Level(TR3LevelNames.WILLIE);
    }

    [TestMethod]
    public void ReadWriteHallows()
    {
        ReadWriteTR3Level(TR3LevelNames.HALLOWS);
    }

    [TestMethod]
    public void ReadWriteFling()
    {
        ReadWriteTR3Level(TR3LevelNames.FLING);
    }

    [TestMethod]
    public void ReadWriteLair()
    {
        ReadWriteTR3Level(TR3LevelNames.LAIR);
    }

    [TestMethod]
    public void ReadWriteCliff()
    {
        ReadWriteTR3Level(TR3LevelNames.CLIFF);
    }

    [TestMethod]
    public void ReadWriteFishes()
    {
        ReadWriteTR3Level(TR3LevelNames.FISHES);
    }
    [TestMethod]
    public void ReadWriteMadhouse()
    {
        ReadWriteTR3Level(TR3LevelNames.MADHOUSE);
    }

    [TestMethod]
    public void ReadWriteReunion()
    {
        ReadWriteTR3Level(TR3LevelNames.REUNION);
    }

    [TestMethod]
    public void Floordata_ReadWrite_DefaultTest()
    {
        TR3Level lvl = GetTR3Level(TR3LevelNames.RXTECH);

        //Store the original floordata from the level
        ushort[] originalFData = new ushort[lvl.NumFloorData];
        Array.Copy(lvl.FloorData, originalFData, lvl.NumFloorData);

        //Parse the floordata using FDControl and re-write the parsed data back
        FDControl fdataReader = new FDControl();
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
        FDControl fdataReader = new FDControl();
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
        Dictionary<int, byte[]> flipOffZones = new Dictionary<int, byte[]>();
        Dictionary<int, byte[]> flipOnZones = new Dictionary<int, byte[]>();
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
        lvl = WriteReadTempLevel(lvl, TR3LevelNames.JUNGLE);

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

        // Store the current list of overlaps
        List<ushort> originalOverlaps = lvl.Overlaps.ToList();

        // For every box, store the current list of overlaps and the overlap starting
        // index itself (which also stores Blockable/Blocked bits).
        Dictionary<int, List<ushort>> boxOverlaps = new Dictionary<int, List<ushort>>();
        Dictionary<int, short> boxOverlapIndices = new Dictionary<int, short>();
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

        Dictionary<int, List<ushort>> newBoxOverlaps = new Dictionary<int, List<ushort>>();
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
        lvl = WriteReadTempLevel(lvl, TR3LevelNames.JUNGLE);

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
            List<int> doors = new List<int>(requiredDoors);
            for (int i = 0; i < requiredDoors; i++)
            {
                doors.Add(i);
            }

            List<TRSecretPlacement<TR3Entities>> secrets = new List<TRSecretPlacement<TR3Entities>>();

            // Create a secret up to the limit for this "level" and set its mask and door
            for (ushort secretIndex = 0; secretIndex < totalSecrets; secretIndex++)
            {
                TRSecretPlacement<TR3Entities> secret = new TRSecretPlacement<TR3Entities>
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
