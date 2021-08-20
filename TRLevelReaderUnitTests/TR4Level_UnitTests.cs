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
        public void Alexhub_ReadTest()
        {
            TR4LevelReader reader = new TR4LevelReader();

            TR4Level lvl = reader.ReadLevel("alexhub.tr4");

            byte[] lvlAsBytes = File.ReadAllBytes("alexhub.tr4");
            byte[] SerializedData = lvl.Serialize();

            //Does our view of the level match byte for byte?
            CollectionAssert.AreEqual(lvlAsBytes, SerializedData, "Read does not match byte for byte");

            TR4LevelWriter writer = new TR4LevelWriter();

            writer.WriteLevelToFile(lvl, "alexhub_TEST.tr2");

            byte[] copyAsBytes = File.ReadAllBytes("alexhub_TEST.tr2");

            //Does our saved copy match the original?
            CollectionAssert.AreEqual(lvlAsBytes, copyAsBytes, "Write does not match byte for byte");
        }
    }
}
