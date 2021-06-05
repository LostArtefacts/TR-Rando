using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TRLevelReader;
using TRLevelReader.Model;

using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using System.Collections.Generic;

namespace TRLevelReaderUnitTests
{
    [TestClass]
    public class TR2Level_UnitTests
    {
        [TestMethod]
        public void GreatWall_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("wall.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("wall.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Venice_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("boat.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("boat.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Bartoli_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("venice.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("venice.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Opera_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("opera.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("opera.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Rig_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("rig.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("rig.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void DA_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("platform.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("platform.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Fathoms_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("unwater.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("unwater.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Doria_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("keel.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("keel.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void LQ_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("living.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("living.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Deck_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("deck.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("deck.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Tibet_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("skidoo.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("skidoo.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void BKang_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("monastry.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("monastry.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Talion_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("catacomb.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("catacomb.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void IcePalace_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("icecave.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("icecave.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Xian_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("emprtomb.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("emprtomb.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Floating_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("floating.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("floating.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Lair_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("xian.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("xian.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void HSH_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("house.tr2");
            byte[] lvlAsBytes = File.ReadAllBytes("house.tr2");

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");

            TR2LevelWriter writer = new TR2LevelWriter();

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void TR3_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();
            Assert.ThrowsException<NotImplementedException>(() => reader.ReadLevel("jungle.tr2"));
        }

        [TestMethod]
        public void Other_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();
            Assert.ThrowsException<NotImplementedException>(() => reader.ReadLevel("joby5.trc"));
        }

        [TestMethod]
        public void FloorData_ReadWriteTest()
        {
            //Read Dragons Lair data
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("xian.tr2");

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

            //Now modify an entry, and ensure it is different to the original data.
            FDPortalEntry portal = fdataReader.Entries[3][0] as FDPortalEntry;
            portal.Room = 42;
            fdataReader.WriteToLevel(lvl);

            //Test. FDIndex 3 of Dragon's Lair is a portal to room 3, which is being modified to Room 42.
            //Data should be:
            //New - [3] = 0x8001 and [4] = 0x002A

            //Get ref to new data
            newFData = lvl.FloorData;

            Assert.AreEqual(newFData[3], (ushort)0x8001);
            Assert.AreEqual(newFData[4], (ushort)0x002A);

            //Compare to make sure the modified fdata was written back.
            CollectionAssert.AreNotEqual(originalFData, newFData, "Floordata matches, change unsuccessful");
            Assert.AreEqual((uint)newFData.Length, lvl.NumFloorData);
        }

        [TestMethod]
        public void FloorData_ReadWriteOneShotTest()
        {
            //Read GW data
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("wall.tr2");

            //Parse the floordata using FDControl
            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Get all triggers for entity ID 18
            List<FDTriggerEntry> triggers = FDUtilities.GetEntityTriggers(fdataReader, 18);

            //Verify none of the triggers have OneShot set
            foreach (FDTriggerEntry trigger in triggers)
            {
                Assert.IsFalse(trigger.TrigSetup.OneShot);
            }

            //Set OneShot on each trigger
            foreach (FDTriggerEntry trigger in triggers)
            {
                trigger.TrigSetup.OneShot = true;
            }

            fdataReader.WriteToLevel(lvl);

            //Save it and read it back in
            TR2LevelWriter writer = new TR2LevelWriter();
            writer.WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

            fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Get the triggers again afresh
            triggers = FDUtilities.GetEntityTriggers(fdataReader, 18);

            //Verify that they now have OneShot set
            foreach (FDTriggerEntry trigger in triggers)
            {
                Assert.IsTrue(trigger.TrigSetup.OneShot);
            }

            //Switch it off again
            foreach (FDTriggerEntry trigger in triggers)
            {
                trigger.TrigSetup.OneShot = false;
            }

            fdataReader.WriteToLevel(lvl);

            //Save it and read it back in
            writer.WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

            fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Get the triggers again afresh
            triggers = FDUtilities.GetEntityTriggers(fdataReader, 18);

            //Verify that they now once again do not have OneShot set
            foreach (FDTriggerEntry trigger in triggers)
            {
                Assert.IsFalse(trigger.TrigSetup.OneShot);
            }
        }
    }
}