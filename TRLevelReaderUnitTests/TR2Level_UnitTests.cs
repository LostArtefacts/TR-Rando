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

            //Set OneShot on each trigger
            foreach (FDTriggerEntry trigger in triggers)
            {
                trigger.TrigSetup.SetOneShot();
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
                trigger.TrigSetup.ClearOneShot();
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
    }
}
