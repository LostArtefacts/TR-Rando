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
using TRLevelReader.Helpers;
using TRLevelReader.Model.Base.Enums;
using System.Drawing;
using TRTexture16Importer;

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

            writer.WriteLevelToFile(lvl, "wall_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("wall_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "boat_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("boat_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "venice_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("venice_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "opera_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("opera_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "rig_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("rig_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "platform_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("platform_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "unwater_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("unwater_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "keel_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("keel_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "living_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("living_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "deck_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("deck_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "skidoo_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("skidoo_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "monastry_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("monastry_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "catacomb_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("catacomb_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "icecave_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("icecave_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "emprtomb_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("emprtomb_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "floating_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("floating_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "xian_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("xian_TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "house_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("house_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
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

            //Test pattern/type matching example for fdata.
            bool isPortal = false;

            foreach (KeyValuePair<int, List<FDEntry>> sector in fdataReader.Entries)
            {
                foreach (FDEntry entry in sector.Value)
                {
                    switch (entry)
                    {
                        case FDClimbEntry climbEntry:
                            break;
                        case FDKillLaraEntry killEntry:
                            break;
                        case FDPortalEntry portalEntry:
                            isPortal = true;
                            break;
                        case FDSlantEntry slantEntry:
                            break;
                        case FDTriggerEntry triggerEntry:
                            break;
                    }
                }
            }

            Assert.IsTrue(isPortal);
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

            //There should be 3
            Assert.AreEqual(triggers.Count, 3);

            //Verify none of the triggers has OneShot set
            foreach (FDTriggerEntry trigger in triggers)
            {
                Assert.IsFalse(trigger.TrigSetup.OneShot);
            }

            //Set OneShot on each trigger and test successive calls
            foreach (FDTriggerEntry trigger in triggers)
            {
                trigger.TrigSetup.OneShot = true;
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

            //Switch it off again and test successive calls
            foreach (FDTriggerEntry trigger in triggers)
            {
                trigger.TrigSetup.OneShot = false;
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

        [TestMethod]
        public void FloorData_ReadWriteTrigTimerTest()
        {
            //Read GW data
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("wall.tr2");

            //Parse the floordata using FDControl
            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Get all triggers for entity ID 18
            List<FDTriggerEntry> triggers = FDUtilities.GetEntityTriggers(fdataReader, 18);

            //There should be 3
            Assert.AreEqual(triggers.Count, 3);

            //Verify none of the triggers has a timer
            foreach (FDTriggerEntry trigger in triggers)
            {
                Assert.AreEqual(trigger.TrigSetup.Timer, 0);
            }

            //Set the timer on each trigger
            for (int i = 0; i < triggers.Count; i++)
            {
                triggers[i].TrigSetup.Timer = (byte)(i * 10);
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

            //Verify that they now have the timers set
            for (int i = 0; i < triggers.Count; i++)
            {
                Assert.AreEqual(triggers[i].TrigSetup.Timer, i * 10);
            }
        }

        [TestMethod]
        public void FloorData_InsertFDTest()
        {
            //Read Dragons Lair data
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("xian.tr2");

            //Parse the floordata using FDControl
            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Find a sector that currently has no floor data
            int room, roomSector = -1;
            for (room = 0; room < lvl.NumRooms; room++)
            {
                roomSector = lvl.Rooms[room].SectorList.ToList().FindIndex(s => s.FDIndex == 0);
                if (roomSector != -1)
                {
                    break;
                }
            }

            if (roomSector == -1)
            {
                Assert.Fail("Could not locate a Room Sector that does not have floor data associated with it.");
            }

            TRRoomSector sector = lvl.Rooms[room].SectorList[roomSector];

            // Create a slot in the FD for this sector
            fdataReader.CreateFloorData(sector);
            Assert.AreNotEqual(sector.FDIndex, 0, "Sector does not have FD allocated.");

            // Add a music trigger
            fdataReader.Entries[sector.FDIndex].Add(new FDTriggerEntry
            {
                Setup = new FDSetup(FDFunctions.Trigger),
                TrigSetup = new FDTrigSetup(),
                TrigActionList = new List<FDActionListItem>
                {
                    new FDActionListItem
                    {
                        TrigAction = FDTrigAction.PlaySoundtrack,
                        Parameter = 40
                    }
                }
            });

            //Write the data back
            fdataReader.WriteToLevel(lvl);

            //Save it and read it back in
            TR2LevelWriter writer = new TR2LevelWriter();
            writer.WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

            //Reassign the sector
            sector = lvl.Rooms[room].SectorList[roomSector];

            fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Ensure the sector still has FD associated with it
            Assert.AreNotEqual(sector.FDIndex, 0, "Sector no longer has FD after write/read.");

            //Verify there is one entry for this sector
            Assert.AreEqual(fdataReader.Entries[sector.FDIndex].Count, 1);

            //Verify the trigger we added matches what we expect
            FDEntry entry = fdataReader.Entries[sector.FDIndex][0];
            Assert.IsTrue(entry is FDTriggerEntry);

            FDTriggerEntry triggerEntry = entry as FDTriggerEntry;
            Assert.IsTrue(triggerEntry.Setup.Function == (byte)FDFunctions.Trigger);
            Assert.IsTrue(triggerEntry.TrigActionList.Count == 1);
            Assert.IsTrue(triggerEntry.TrigActionList[0].TrigAction == FDTrigAction.PlaySoundtrack);
            Assert.IsTrue(triggerEntry.TrigActionList[0].Parameter == 40);
        }

        [TestMethod]
        public void FloorData_RemoveFDTest()
        {
            //Read Dragons Lair data
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("xian.tr2");

            //Parse the floordata using FDControl
            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Find a sector that currently has floor data
            int room, roomSector = -1;
            for (room = 0; room < lvl.NumRooms; room++)
            {
                roomSector = lvl.Rooms[room].SectorList.ToList().FindIndex(s => s.FDIndex > 0);
                if (roomSector != -1)
                {
                    break;
                }
            }

            if (roomSector == -1)
            {
                Assert.Fail("Could not locate a Room Sector that has floor data associated with it.");
            }

            TRRoomSector sector = lvl.Rooms[room].SectorList[roomSector];

            // Remove the FD for this sector
            fdataReader.RemoveFloorData(sector);
            Assert.AreEqual(sector.FDIndex, 0, "Sector still has FD allocated.");

            //Write the data back
            fdataReader.WriteToLevel(lvl);

            //Save it and read it back in
            TR2LevelWriter writer = new TR2LevelWriter();
            writer.WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

            //Reassign the sector
            sector = lvl.Rooms[room].SectorList[roomSector];

            fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Ensure the sector still has FD associated with it
            Assert.AreEqual(sector.FDIndex, 0, "Sector still has FD after write/read.");
        }

        [TestMethod]
        public void FloorData_InsertRemoveFDEntryTest()
        {
            //Read Dragons Lair data
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("xian.tr2");

            //Store the original floordata from the level
            ushort[] originalFData = new ushort[lvl.NumFloorData];
            Array.Copy(lvl.FloorData, originalFData, lvl.NumFloorData);

            //Parse the floordata using FDControl
            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Verify index 9 has one entry and that it's currently 
            //set as EndData for this index
            Assert.AreEqual(fdataReader.Entries[9].Count, 1);
            Assert.IsTrue(fdataReader.Entries[9][0].Setup.EndData);

            //Verify the next index is currently 9 + the entry's length
            List<int> indices = fdataReader.Entries.Keys.ToList();
            int nextIndex = 9 + fdataReader.Entries[9][0].Flatten().Length;
            Assert.AreEqual(nextIndex, indices[indices.IndexOf(9) + 1]);

            //Add a music trigger to index 9
            fdataReader.Entries[9].Add(new FDTriggerEntry
            {
                Setup = new FDSetup(FDFunctions.Trigger),
                TrigSetup = new FDTrigSetup(),
                TrigActionList = new List<FDActionListItem>
                {
                    new FDActionListItem
                    {
                        TrigAction = FDTrigAction.PlaySoundtrack,
                        Parameter = 40
                    }
                }
            });

            //Write the data back
            fdataReader.WriteToLevel(lvl);

            //Verify index 9 has two entries, that its first entry 
            //does not have EndData set, but that its second does
            Assert.AreEqual(fdataReader.Entries[9].Count, 2);
            Assert.IsFalse(fdataReader.Entries[9][0].Setup.EndData);
            Assert.IsTrue(fdataReader.Entries[9][1].Setup.EndData);

            //Verify the next index is now 9 + both the entry's lengths
            //Bear in mind the underlying dictionary's keys have changed
            indices = fdataReader.Entries.Keys.ToList();
            nextIndex = 9 + fdataReader.Entries[9][0].Flatten().Length + fdataReader.Entries[9][1].Flatten().Length;
            Assert.AreEqual(nextIndex, indices[indices.IndexOf(9) + 1]);

            //Remove the new entry
            fdataReader.Entries[9].RemoveAt(1);

            //Write the data back
            fdataReader.WriteToLevel(lvl);

            //Verify index 9 again has one entry and that it's again 
            //set as EndData for this index
            Assert.AreEqual(fdataReader.Entries[9].Count, 1);
            Assert.IsTrue(fdataReader.Entries[9][0].Setup.EndData);

            //Verify the next index is again 9 + the entry's length
            indices = fdataReader.Entries.Keys.ToList();
            nextIndex = 9 + fdataReader.Entries[9][0].Flatten().Length;
            Assert.AreEqual(nextIndex, indices[indices.IndexOf(9) + 1]);

            //Finally compare to make sure the original fdata was written back.
            CollectionAssert.AreEqual(originalFData, lvl.FloorData, "Floordata does not match");
            Assert.AreEqual((uint)lvl.FloorData.Length, lvl.NumFloorData);
        }

        [TestMethod]
        public void FloorData_InsertFDEntryWriteReadTest()
        {
            //Read Dragons Lair data
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("xian.tr2");

            //Parse the floordata using FDControl
            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Add a music trigger to index 9
            fdataReader.Entries[9].Add(new FDTriggerEntry
            {
                Setup = new FDSetup(FDFunctions.Trigger),
                TrigSetup = new FDTrigSetup(),
                TrigActionList = new List<FDActionListItem>
                {
                    new FDActionListItem
                    {
                        TrigAction = FDTrigAction.PlaySoundtrack,
                        Parameter = 40
                    }
                }
            });

            //Write the data back
            fdataReader.WriteToLevel(lvl);

            //Save it and read it back in
            TR2LevelWriter writer = new TR2LevelWriter();
            writer.WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

            fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Verify index 9 has two entries, that its first entry 
            //does not have EndData set, but that its second does
            Assert.AreEqual(fdataReader.Entries[9].Count, 2);
            Assert.IsFalse(fdataReader.Entries[9][0].Setup.EndData);
            Assert.IsTrue(fdataReader.Entries[9][1].Setup.EndData);

            //Verify the trigger we added matches what we expect
            FDEntry entry = fdataReader.Entries[9][1];
            Assert.IsTrue(entry is FDTriggerEntry);

            FDTriggerEntry triggerEntry = entry as FDTriggerEntry;
            Assert.IsTrue(triggerEntry.Setup.Function == (byte)FDFunctions.Trigger);
            Assert.IsTrue(triggerEntry.TrigActionList.Count == 1);
            Assert.IsTrue(triggerEntry.TrigActionList[0].TrigAction == FDTrigAction.PlaySoundtrack);
            Assert.IsTrue(triggerEntry.TrigActionList[0].Parameter == 40);
        }

        [TestMethod]
        public void FloorData_AppendFDActionListItemTest()
        {
            //Read Dragons Lair data
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("xian.tr2");

            //Parse the floordata using FDControl
            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Add a music action to the trigger at index 13
            FDTriggerEntry trigger = fdataReader.Entries[13][0] as FDTriggerEntry;
            Assert.AreEqual(trigger.TrigActionList.Count, 2);
            trigger.TrigActionList.Add(new FDActionListItem
            {
                TrigAction = FDTrigAction.PlaySoundtrack,
                Parameter = 40
            });

            //Write the data back
            fdataReader.WriteToLevel(lvl);

            //Save it and read it back in
            TR2LevelWriter writer = new TR2LevelWriter();
            writer.WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

            fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            trigger = fdataReader.Entries[13][0] as FDTriggerEntry;
            // Verifying that the trigger has 3 items implicitly verifies that the Continue
            // flag was correctly changed on the previous last item and on the new item,
            // otherwise the parsing would have stopped at the second
            Assert.AreEqual(trigger.TrigActionList.Count, 3);

            Assert.IsTrue(trigger.TrigActionList[2].TrigAction == FDTrigAction.PlaySoundtrack);
            Assert.IsTrue(trigger.TrigActionList[2].Parameter == 40);
        }

        [TestMethod]
        public void FloorData_AppendFDActionListItemCamTest()
        {
            //Read Dragons Lair data
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("xian.tr2");

            //Parse the floordata using FDControl
            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            //Add a music action to the trigger at index 6010
            //This has a CamAction in its TrigList so this tests
            //that the Continue flag is correctly set
            FDTriggerEntry trigger = fdataReader.Entries[6010][1] as FDTriggerEntry;
            Assert.AreEqual(trigger.TrigActionList.Count, 2);
            Assert.IsNotNull(trigger.TrigActionList[1].CamAction);
            Assert.IsFalse(trigger.TrigActionList[1].CamAction.Continue);

            trigger.TrigActionList.Add(new FDActionListItem
            {
                TrigAction = FDTrigAction.PlaySoundtrack,
                Parameter = 40
            });

            //Write the data back
            fdataReader.WriteToLevel(lvl);

            //Check the CamAction has been updated
            Assert.AreEqual(trigger.TrigActionList.Count, 3);
            Assert.IsNotNull(trigger.TrigActionList[1].CamAction);
            Assert.IsTrue(trigger.TrigActionList[1].CamAction.Continue);

            //Check the music trigger has Continue set to false
            Assert.IsFalse(trigger.TrigActionList[2].Continue);
        }

        [TestMethod]
        public void FloorData_ModifyClimbableTest()
        {
            // Get original ladders in +/-X directions
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("wall.tr2");

            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);
            
            // Second guardhouse ladder
            FDClimbEntry negXEntry = fdataReader.Entries[577][0] as FDClimbEntry;
            // TRex pit ladder
            FDClimbEntry posXEntry = fdataReader.Entries[7405][0] as FDClimbEntry;

            // Confirm both are/are not end data for the sake of comparing the value
            Assert.AreEqual(negXEntry.Setup.EndData, posXEntry.Setup.EndData);

            ushort negXValue = negXEntry.Setup.Value;
            ushort posXValue = posXEntry.Setup.Value;

            // Confirm they are currently what we expect
            Assert.IsTrue(negXEntry.IsNegativeX);
            Assert.IsFalse(negXEntry.IsPositiveX);
            Assert.IsTrue(posXEntry.IsPositiveX);
            Assert.IsFalse(posXEntry.IsNegativeX);

            // Flip the negative X entry and confirm it's now positive only
            negXEntry.IsPositiveX = true;
            negXEntry.IsNegativeX = false;
            Assert.IsTrue(negXEntry.IsPositiveX);
            Assert.IsFalse(negXEntry.IsNegativeX);
            Assert.AreEqual(negXEntry.Setup.Value, posXValue);

            // Flip the positive X entry and confirm it's now negative only
            posXEntry.IsPositiveX = false;
            posXEntry.IsNegativeX = true;
            Assert.IsTrue(posXEntry.IsNegativeX);
            Assert.IsFalse(posXEntry.IsPositiveX);
            Assert.AreEqual(posXEntry.Setup.Value, negXValue);


            // Get original ladders in +/-Z directions
            lvl = reader.ReadLevel("venice.tr2");
            fdataReader.ParseFromLevel(lvl);

            // Room 79
            FDClimbEntry negZEntry = fdataReader.Entries[2239][0] as FDClimbEntry;
            FDClimbEntry posZEntry = fdataReader.Entries[2228][0] as FDClimbEntry;

            // Confirm both are/are not end data for the sake of comparing the value
            Assert.AreEqual(negXEntry.Setup.EndData, posXEntry.Setup.EndData);

            ushort negZValue = negZEntry.Setup.Value;
            ushort posZValue = posZEntry.Setup.Value;

            // Confirm they are currently what we expect
            Assert.IsTrue(negZEntry.IsNegativeZ);
            Assert.IsFalse(negZEntry.IsPositiveZ);
            Assert.IsTrue(posZEntry.IsPositiveZ);
            Assert.IsFalse(posZEntry.IsNegativeZ);

            // Flip the negative Z entry and confirm it's now positive only
            negZEntry.IsPositiveZ = true;
            negZEntry.IsNegativeZ = false;
            Assert.IsTrue(negZEntry.IsPositiveZ);
            Assert.IsFalse(negZEntry.IsNegativeZ);
            Assert.AreEqual(negZEntry.Setup.Value, posZValue);

            // Flip the positive Z entry and confirm it's now negative only
            posZEntry.IsPositiveZ = false;
            posZEntry.IsNegativeZ = true;
            Assert.IsTrue(posZEntry.IsNegativeZ);
            Assert.IsFalse(posZEntry.IsPositiveZ);
            Assert.AreEqual(posZEntry.Setup.Value, negZValue);


            // Get original ladders with more than one direction
            // Room 58
            FDClimbEntry negZNegXEntry = fdataReader.Entries[1432][0] as FDClimbEntry;
            FDClimbEntry posZNegXEntry = fdataReader.Entries[1437][0] as FDClimbEntry;

            // Confirm both are/are not end data for the sake of comparing the value
            Assert.AreEqual(negZNegXEntry.Setup.EndData, posZNegXEntry.Setup.EndData);

            ushort negZNegXValue = negZNegXEntry.Setup.Value;
            ushort posZNegXValue = posZNegXEntry.Setup.Value;

            // Confirm they are currently what we expect
            Assert.IsTrue(negZNegXEntry.IsNegativeZ);
            Assert.IsTrue(negZNegXEntry.IsNegativeX);
            Assert.IsFalse(negZNegXEntry.IsPositiveZ);
            Assert.IsFalse(negZNegXEntry.IsPositiveX);

            Assert.IsTrue(posZNegXEntry.IsPositiveZ);
            Assert.IsTrue(posZNegXEntry.IsNegativeX);
            Assert.IsFalse(posZNegXEntry.IsNegativeZ);
            Assert.IsFalse(posZNegXEntry.IsPositiveX);

            // Flip the negative Z and confirm it matches the posZNegXValue entry
            negZNegXEntry.IsPositiveZ = true;
            negZNegXEntry.IsNegativeZ = false;
            negZNegXEntry.IsPositiveX = false;
            negZNegXEntry.IsNegativeX = true;
            Assert.IsTrue(negZNegXEntry.IsPositiveZ);
            Assert.IsTrue(negZNegXEntry.IsNegativeX);
            Assert.IsFalse(negZNegXEntry.IsNegativeZ);
            Assert.IsFalse(negZNegXEntry.IsPositiveX);
            Assert.AreEqual(negZNegXEntry.Setup.Value, posZNegXValue);

            // Flip the positive Z and confirm it matches the posZNegXValue entry
            posZNegXEntry.IsPositiveZ = false;
            posZNegXEntry.IsNegativeZ = true;
            posZNegXEntry.IsPositiveX = false;
            posZNegXEntry.IsNegativeX = true;
            Assert.IsTrue(posZNegXEntry.IsNegativeZ);
            Assert.IsTrue(posZNegXEntry.IsNegativeX);
            Assert.IsFalse(posZNegXEntry.IsPositiveZ);
            Assert.IsFalse(posZNegXEntry.IsPositiveX);
            Assert.AreEqual(posZNegXEntry.Setup.Value, negZNegXValue);
        }

        [TestMethod]
        public void FloorData_ModifySlantsTest()
        {
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("wall.tr2");

            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            // Get a sector that is slanted in both X and Z directions
            TRRoomSector sector = FDUtilities.GetRoomSector(61891, 3129, 24010, 35, lvl, fdataReader);
            List<FDEntry> entries = fdataReader.Entries[sector.FDIndex];

            // Confirm we have a match of what we expect
            Assert.AreEqual(entries.Count, 1);
            Assert.IsTrue(entries[0] is FDSlantEntry);
            FDSlantEntry slantEntry = entries[0] as FDSlantEntry;

            // Check current values are X=-2, Z=-3
            Assert.AreEqual(slantEntry.XSlant, -2);
            Assert.AreEqual(slantEntry.ZSlant, -3);

            // Change X only
            slantEntry.XSlant--;
            Assert.AreEqual(slantEntry.XSlant, -3);
            Assert.AreEqual(slantEntry.ZSlant, -3);

            // Change Z only
            slantEntry.ZSlant++;
            Assert.AreEqual(slantEntry.XSlant, -3);
            Assert.AreEqual(slantEntry.ZSlant, -2);

            // Change X sign
            slantEntry.XSlant *= -1;
            Assert.AreEqual(slantEntry.XSlant, 3);
            Assert.AreEqual(slantEntry.ZSlant, -2);

            // Change Z sign
            slantEntry.ZSlant *= -1;
            Assert.AreEqual(slantEntry.XSlant, 3);
            Assert.AreEqual(slantEntry.ZSlant, 2);

            // Write to level and confirm values remain the same on reload
            fdataReader.WriteToLevel(lvl);

            // Save it and read it back in
            TR2LevelWriter writer = new TR2LevelWriter();
            writer.WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

            fdataReader.ParseFromLevel(lvl);

            slantEntry = fdataReader.Entries[sector.FDIndex][0] as FDSlantEntry;
            Assert.AreEqual(slantEntry.XSlant, 3);
            Assert.AreEqual(slantEntry.ZSlant, 2);

            // Make a new entry
            sector = FDUtilities.GetRoomSector(64044, 5632, 32440, 36, lvl, fdataReader);
            Assert.AreEqual(sector.FDIndex, 0);
            fdataReader.CreateFloorData(sector);

            slantEntry = new FDSlantEntry
            {
                Setup = new FDSetup { Value = 2 },
                Type = FDSlantEntryType.FloorSlant,
                SlantValue = 0
            };
            fdataReader.Entries[sector.FDIndex].Add(slantEntry);

            Assert.AreEqual(slantEntry.XSlant, 0);
            Assert.AreEqual(slantEntry.ZSlant, 0);

            slantEntry.XSlant = 1;
            slantEntry.ZSlant = -2;
            Assert.AreEqual(slantEntry.XSlant, 1);
            Assert.AreEqual(slantEntry.ZSlant, -2);

            // Write to level and confirm values remain the same on reload
            fdataReader.WriteToLevel(lvl);

            // Save it and read it back in
            writer.WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

            fdataReader.ParseFromLevel(lvl);

            slantEntry = fdataReader.Entries[sector.FDIndex][0] as FDSlantEntry;
            Assert.AreEqual(slantEntry.XSlant, 1);
            Assert.AreEqual(slantEntry.ZSlant, -2);

            // Edge cases
            slantEntry.XSlant = -1;
            slantEntry.ZSlant = -1;
            Assert.AreEqual(slantEntry.XSlant, -1);
            Assert.AreEqual(slantEntry.ZSlant, -1);

            slantEntry.XSlant = -1;
            slantEntry.ZSlant = 0;
            Assert.AreEqual(slantEntry.XSlant, -1);
            Assert.AreEqual(slantEntry.ZSlant, 0);

            slantEntry.XSlant = 0;
            slantEntry.ZSlant = -1;
            Assert.AreEqual(slantEntry.XSlant, 0);
            Assert.AreEqual(slantEntry.ZSlant, -1);

            slantEntry.XSlant = 1;
            slantEntry.ZSlant = 0;
            Assert.AreEqual(slantEntry.XSlant, 1);
            Assert.AreEqual(slantEntry.ZSlant, 0);

            slantEntry.XSlant = 0;
            slantEntry.ZSlant = 1;
            Assert.AreEqual(slantEntry.XSlant, 0);
            Assert.AreEqual(slantEntry.ZSlant, 1);

            slantEntry.XSlant = 1;
            slantEntry.ZSlant = 1;
            Assert.AreEqual(slantEntry.XSlant, 1);
            Assert.AreEqual(slantEntry.ZSlant, 1);
        }

        [TestMethod]
        public void FloorData_ModifyTriggerMask()
        {
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("monastry.tr2");

            FDControl fdataReader = new FDControl();
            fdataReader.ParseFromLevel(lvl);

            // For the end doors to open in Barkhang, all 5 activation bits must be set.
            // Check that each trigger exclusively has one of these bits, so 2^0 to 2^4.
            // The sum should be 31.

            int[] slots = new int[] { 0, 3, 4, 9, 10 };
            int mask = 0;
            foreach (int slotIndex in slots)
            {
                TR2Entity slot = lvl.Entities[slotIndex];
                TRRoomSector sector = FDUtilities.GetRoomSector(slot.X, slot.Y, slot.Z, slot.Room, lvl, fdataReader);

                // Confirm we have a match of what we expect
                Assert.AreNotEqual(sector.FDIndex, 0);
                List<FDEntry> entries = fdataReader.Entries[sector.FDIndex];
                Assert.AreEqual(entries.Count, 1);
                Assert.IsTrue(entries[0] is FDTriggerEntry);

                FDTriggerEntry trigger = entries[0] as FDTriggerEntry;
                // Confirm this mask will change the overall check mask
                int newMask = mask | trigger.TrigSetup.Mask;
                Assert.AreNotEqual(mask, newMask);
                mask = newMask;
            }

            // Final check the mask matches full activation
            Assert.AreEqual(mask, 31);

            // Test changing a mask value
            lvl = reader.ReadLevel("keel.tr2");
            fdataReader.ParseFromLevel(lvl);

            TR2Entity circuitBreakerSlot = lvl.Entities[15];
            TRRoomSector cbSector = FDUtilities.GetRoomSector(circuitBreakerSlot.X, circuitBreakerSlot.Y, circuitBreakerSlot.Z, circuitBreakerSlot.Room, lvl, fdataReader);

            // Confirm we have a match of what we expect
            Assert.AreNotEqual(cbSector.FDIndex, 0);
            List<FDEntry> cbEntries = fdataReader.Entries[cbSector.FDIndex];
            Assert.AreEqual(cbEntries.Count, 1);
            Assert.IsTrue(cbEntries[0] is FDTriggerEntry);

            FDTriggerEntry cbTrigger = cbEntries[0] as FDTriggerEntry;

            // We expect a normal mask for Wreck so all 1's set
            Assert.AreEqual(cbTrigger.TrigSetup.Mask, 31);

            // Take off 2 bits. This simulates having something activated
            // only after all 3 breakers are used.
            // 16 8 4 2 1
            // ----------
            //  1 0 0 1 1
            //  0 1 0 1 1
            //  0 0 1 1 1
            cbTrigger.TrigSetup.Mask = 7;
            Assert.AreEqual(cbTrigger.TrigSetup.Mask, 7);

            // Save the level and re-read it to confirm it still matches.
            fdataReader.WriteToLevel(lvl);
            new TR2LevelWriter().WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

            fdataReader.ParseFromLevel(lvl);

            cbSector = FDUtilities.GetRoomSector(circuitBreakerSlot.X, circuitBreakerSlot.Y, circuitBreakerSlot.Z, circuitBreakerSlot.Room, lvl, fdataReader);

            // Confirm we have a match of what we expect
            Assert.AreNotEqual(cbSector.FDIndex, 0);
            cbEntries = fdataReader.Entries[cbSector.FDIndex];
            Assert.AreEqual(cbEntries.Count, 1);
            Assert.IsTrue(cbEntries[0] is FDTriggerEntry);

            cbTrigger = cbEntries[0] as FDTriggerEntry;

            Assert.AreEqual(cbTrigger.TrigSetup.Mask, 7);
        }

        [TestMethod]
        public void ModifyZonesTest()
        {
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("wall.tr2");

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
            new TR2LevelWriter().WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

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
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("wall.tr2");

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

            // Confirm the total matches the total number of overlaps in the level.
            int total = 0;
            boxOverlaps.Values.ToList().ForEach(v => total += v.Count);
            Assert.AreEqual(lvl.NumOverlaps, (uint)total);

            // Write everything back with no changes.
            for (int i = 0; i < lvl.NumBoxes; i++)
            {
                TR2BoxUtilities.UpdateOverlaps(lvl, lvl.Boxes[i], boxOverlaps[i]);
            }

            // Confirm the level overlap list is identical
            CollectionAssert.AreEqual(originalOverlaps, lvl.Overlaps.ToList());

            // Confirm the box overlap indices are identical
            for (int i = 0; i < lvl.NumBoxes; i++)
            {
                Assert.AreEqual(boxOverlapIndices[i], lvl.Boxes[i].OverlapIndex);
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
            new TR2LevelWriter().WriteLevelToFile(lvl, "TEST.tr2");
            lvl = reader.ReadLevel("TEST.tr2");

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
        public void ModifyTexturesTest()
        {
            TR2LevelReader reader = new TR2LevelReader();
            TR2Level lvl = reader.ReadLevel("monastry.tr2");

            // Store the untouched raw data
            byte[] lvlAsBytes = lvl.Serialize();

            // Convert each tile to a bitmap, and then convert it back
            foreach (TRTexImage16 tile in lvl.Images16)
            {
                using (Bitmap bmp = tile.ToBitmap())
                {
                    tile.Pixels = TextureUtilities.ImportFromBitmap(bmp);
                }
            }

            // Confirm the raw data still matches
            CollectionAssert.AreEqual(lvlAsBytes, lvl.Serialize(), "Read does not match byte for byte");
        }
    }
}
