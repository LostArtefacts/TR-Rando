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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

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

            writer.WriteLevelToFile(lvl, "TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }
    }
}
