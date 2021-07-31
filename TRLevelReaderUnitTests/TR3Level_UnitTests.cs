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
    }
}
