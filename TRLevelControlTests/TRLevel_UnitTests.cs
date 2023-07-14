using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

using TRLevelReader;
using TRLevelReader.Model;

using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using System.Linq;
using TRLevelReader.Model.Base.Enums;
using TRLevelReader.Helpers;
using TRModelTransporter.Handlers;

namespace TRLevelReaderUnitTests
{
    [TestClass]
    public class TRLevel_UnitTests
    {
        [TestMethod]
        public void Level1_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level1.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level1.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level1_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level1_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level2_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level2.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level2.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level2_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level2_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level3A_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level3a.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level3a.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level3a_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level3a_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level3B_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level3b.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level3b.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level3b_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level3b_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level4_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level4.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level4.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level4_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level4_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level5_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level5.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level5.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level5_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level5_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level6_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level6.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level6.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level6_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level6_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level7A_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level7a.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level7a.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level7a_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level7a_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level7B_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level7b.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level7b.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level7b_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level7b_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level8A_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level8a.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level8a.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level8a_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level8a_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level8B_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level8b.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level8b.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level8b_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level8b_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level8C_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level8c.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level8c.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level8c_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level8c_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level10A_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level10a.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level10a.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level10a_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level10a_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level10B_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level10b.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level10b.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level10b_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level10b_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Level10C_UnitTest()
        {
            TR1LevelReader reader = new TR1LevelReader();

            TRLevel lvl = reader.ReadLevel("level10c.phd");

            byte[] lvlAsBytes = File.ReadAllBytes("level10c.phd");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR1LevelWriter writer = new TR1LevelWriter();

            writer.WriteLevelToFile(lvl, "level10c_TEST.phd");

            byte[] copyAsBytes = File.ReadAllBytes("level10c_TEST.phd");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Floordata_ReadWrite_DefaultTest()
        {
            TR1LevelReader reader = new TR1LevelReader();
            TRLevel lvl = reader.ReadLevel("level10c.phd");

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

            TR1LevelWriter writer = new TR1LevelWriter();
            writer.WriteLevelToFile(lvl, "level10c_fdata.phd");
        }

        [TestMethod]
        public void ModifyZonesTest()
        {
            TR1LevelReader reader = new TR1LevelReader();
            TRLevel lvl = reader.ReadLevel("level1.phd");

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
            List<TRBox> boxes = lvl.Boxes.ToList();
            boxes.Add(boxes[0]);
            lvl.Boxes = boxes.ToArray();
            lvl.NumBoxes++;

            // Add a new zone for the box and store its serialized form for comparison
            int newBoxIndex = (int)(lvl.NumBoxes - 1);
            TR1BoxUtilities.DuplicateZone(lvl, 0);
            flipOffZones[newBoxIndex] = lvl.Zones[newBoxIndex][FlipStatus.Off].Serialize();
            flipOnZones[newBoxIndex] = lvl.Zones[newBoxIndex][FlipStatus.On].Serialize();

            // Verify the number of zone ushorts matches what's expected for the box count
            Assert.AreEqual(TR1BoxUtilities.FlattenZones(lvl.Zones).Length, (int)(6 * lvl.NumBoxes));

            // Write and re-read the level
            new TR1LevelWriter().WriteLevelToFile(lvl, "TEST.phd");
            lvl = reader.ReadLevel("TEST.phd");

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
        public void ResortSoundsTest()
        {
            TR1LevelReader reader = new TR1LevelReader();
            TRLevel lvl = reader.ReadLevel("level1.phd");

            byte[] lvlBeforeSort = lvl.Serialize();

            SoundUtilities.ResortSoundIndices(lvl);
            
            byte[] lvlAfterSort = lvl.Serialize();

            CollectionAssert.AreEqual(lvlBeforeSort, lvlAfterSort);
        }
    }
}
