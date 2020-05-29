using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TRLevelReader;
using TRLevelReader.Model;

namespace TRLevelReaderUnitTests
{
    [TestClass]
    public class TR2Level_UnitTests
    {
        private static readonly uint TR2VersionHeader = 0x0000002D;

        [TestMethod]
        public void GreatWall_ReadTest()
        {
            TR2Level lvl = TR2LevelReader.ReadLevel("wall.tr2");

            Assert.AreEqual(TR2VersionHeader, lvl.Version);
        }

        [TestMethod]
        public void TR3_ReadTest()
        {
            Assert.ThrowsException<NotImplementedException>(() => TR2LevelReader.ReadLevel("jungle.tr2"));
        }

        [TestMethod]
        public void Other_ReadTest()
        {
            Assert.ThrowsException<NotImplementedException>(() => TR2LevelReader.ReadLevel("joby5.trc"));
        }
    }
}
