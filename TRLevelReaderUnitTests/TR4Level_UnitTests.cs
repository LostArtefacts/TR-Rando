using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using TRLevelReader;
using TRLevelReader.Model;

namespace TRLevelReaderUnitTests
{
    [TestClass]
    public class TR4Level_UnitTests
    {
        [TestMethod]
        public void Ankgor1_ReadTest()
        {
            TR4LevelReader reader = new TR4LevelReader();

            TR4Level lvl = reader.ReadLevel("angkor1.tr4");

            byte[] lvlAsBytes = File.ReadAllBytes("angkor1.tr4");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            //CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR4LevelWriter writer = new TR4LevelWriter();

            writer.WriteLevelToFile(lvl, "angkor1_TEST.tr4");

            byte[] copyAsBytes = File.ReadAllBytes("angkor1_TEST.tr4");

            lvl = reader.ReadLevel("angkor1_TEST.tr4");
            SerializedData = lvl.Serialize();

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }
    }
}
