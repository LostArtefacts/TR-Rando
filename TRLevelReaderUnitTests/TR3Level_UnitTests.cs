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

namespace TRLevelReaderUnitTests
{
    [TestClass]
    public class TR3Level_UnitTests
    {
        [TestMethod]
        public void Jungle_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("jungle.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("jungle.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "jungle_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("jungle_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Ruins_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("temple.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("temple.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "temple_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("temple_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Ganges_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("quadchas.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("quadchas.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "quadchas_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("quadchas_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Kaliya_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("tonyboss.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("tonyboss.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "tonyboss_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("tonyboss_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Nevada_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("nevada.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("nevada.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "nevada_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("nevada_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void HSC_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("compound.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("compound.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "compound_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("compound_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Area51_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("area51.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("area51.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "area51_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("area51_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Thames_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("roofs.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("roofs.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "roofs_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("roofs_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Aldwych_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("sewer.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("sewer.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "sewer_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("sewer_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Luds_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("tower.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("tower.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "tower_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("tower_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void City_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("office.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("office.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "office_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("office_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Coastal_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("shore.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("shore.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "shore_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("shore_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Crash_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("crash.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("crash.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "crash_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("crash_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Madubu_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("rapids.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("rapids.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "rapids_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("rapids_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Puna_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("triboss.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("triboss.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "triboss_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("triboss_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Antarctica_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("antarc.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("antarc.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "antarc_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("antarc_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void RX_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("mines.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("mines.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "mines_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("mines_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Tinnos_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("city.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("city.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "city_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("city_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Cavern_ReadTest()
        {
            TR3LevelReader reader = new TR3LevelReader();

            TR3Level lvl = reader.ReadLevel("chamber.tr2");

            byte[] lvlAsBytes = File.ReadAllBytes("chamber.tr2");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR3LevelWriter writer = new TR3LevelWriter();

            writer.WriteLevelToFile(lvl, "chamber_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("chamber_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }

        [TestMethod]
        public void Floordata_ReadWrite_DefaultTest()
        {
            TR3LevelReader reader = new TR3LevelReader();
            TR3Level lvl = reader.ReadLevel("mines.tr2");

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

            TR3LevelWriter writer = new TR3LevelWriter();
            writer.WriteLevelToFile(lvl, "mines_fdata.tr2");
        }

        [TestMethod]
        public void Floordata_ReadWrite_LevelHasMonkeySwingTest()
        {
            TR3LevelReader reader = new TR3LevelReader();
            TR3Level lvl = reader.ReadLevel("roofs.tr2");

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

            TR3LevelWriter writer = new TR3LevelWriter();
            writer.WriteLevelToFile(lvl, "roofs_fdata.tr2");
        }

        [TestMethod]
        public void Compound_CreateMP_Map()
        {
            //This unit test is specific to TR3MP.
            TR3LevelReader reader = new TR3LevelReader();
            TR3Level lvl = reader.ReadLevel("compound.tr2");

            //Fixed data common to all lara "respawn points"
            const short TypeID = 0;
            const short Intensity1 = -1;
            const short Intensity2 = -1;
            const ushort Flags = 0;
            const short Angle = 0;

            //Replace entities with Laras to make respawn points. Remove any spares.
            lvl.Entities[0] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 1, X = 13579, Y = 0, Z = 24288 };
            lvl.Entities[4] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 0, X = 10733, Y = 0, Z = 24384 };
            lvl.Entities[5] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 113, X = 7562, Y = 0, Z = 24501 };
            lvl.Entities[6] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 3, X = 7582, Y = 0, Z = 37208 };
            lvl.Entities[7] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 4, X = 10816, Y = 0, Z = 37321 };
            lvl.Entities[12] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 5, X = 13784, Y = 0, Z = 37582 };
            lvl.Entities[13] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 6, X = 16989, Y = 0, Z = 37582 };
            lvl.Entities[15] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 7, X = 5617, Y = 2304, Z = 32118 };
            lvl.Entities[16] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 10, X = 22993, Y = 2048, Z = 34402 };
            lvl.Entities[17] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 50, X = 20990, Y = 2048, Z = 25048 };
            lvl.Entities[19] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 32, X = 19933, Y = 2048, Z = 12926 };
            lvl.Entities[20] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 12, X = 28165, Y = 2816, Z = 16902 };
            lvl.Entities[21] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 13, X = 26153, Y = 2304, Z = 21143 };
            lvl.Entities[22] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 14, X = 28198, Y = 2137, Z = 23193 };
            lvl.Entities[23] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 14, X = 26039, Y = 2121, Z = 23529 };
            lvl.Entities[31] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 18, X = 28281, Y = 2048, Z = 27213 };
            lvl.Entities[72] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 16, X = 31210, Y = 2048, Z = 35241 };
            lvl.Entities[156] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 16, X = 31091, Y = 2048, Z = 28165 };
            lvl.Entities[157] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 19, X = 33272, Y = 2048, Z = 31168 };
            lvl.Entities[158] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 19, X = 41527, Y = 2048, Z = 31144 };
            lvl.Entities[159] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 19, X = 41438, Y = 1792, Z = 38471 };
            lvl.Entities[160] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 19, X = 33295, Y = 1792, Z = 38428 };
            lvl.Entities[161] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 19, X = 35309, Y = 1792, Z = 33382 };
            lvl.Entities[162] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 19, X = 37424, Y = 1792, Z = 33203 };
            lvl.Entities[163] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 19, X = 39506, Y = 1792, Z = 33095 };
            lvl.Entities[164] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 16, X = 30112, Y = 2048, Z = 32231 };
            lvl.Entities[165] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 11, X = 28131, Y = 2048, Z = 33311 };
            lvl.Entities[166] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 7, X = 17989, Y = 2048, Z = 26994 };
            lvl.Entities[167] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 7, X = 6690, Y = 2048, Z = 27149 };
            lvl.Entities[168] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 117, X = 11697, Y = 0, Z = 30239 };
            lvl.Entities[169] = new TR2Entity { TypeID = TypeID, Intensity1 = Intensity1, Intensity2 = Intensity2, Flags = Flags, Angle = Angle, Room = 117, X = 11669, Y = 0, Z = 31155 };

            TR3LevelWriter writer = new TR3LevelWriter();
            writer.WriteLevelToFile(lvl, "hscdm.tr2");
        }

        [TestMethod]
        public void ModifyZonesTest()
        {
            TR3LevelReader reader = new TR3LevelReader();
            TR3Level lvl = reader.ReadLevel("jungle.tr2");

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
            new TR3LevelWriter().WriteLevelToFile(lvl, "TEST.tr2");
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
            TR3LevelReader reader = new TR3LevelReader();
            TR3Level lvl = reader.ReadLevel("jungle.tr2");

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
            new TR3LevelWriter().WriteLevelToFile(lvl, "TEST.tr2");
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
    }
}
