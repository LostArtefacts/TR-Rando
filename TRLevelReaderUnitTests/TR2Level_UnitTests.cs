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
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("wall.tr2");

            //Images/Textures - ToDo actual content of them against expected.
            Assert.AreEqual((uint)11, lvl.NumImages);

            //Rooms
            Assert.AreEqual((ushort)84, lvl.NumRooms);
        }

        [TestMethod]
        public void Venice_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("boat.tr2");

            Assert.AreEqual((uint)133, lvl.NumEntities);
        }

        [TestMethod]
        public void Bartoli_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("venice.tr2");

            Assert.AreEqual((uint)134, lvl.NumEntities);
        }

        [TestMethod]
        public void Opera_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("opera.tr2");

            Assert.AreEqual((uint)216, lvl.NumEntities);
        }

        [TestMethod]
        public void Rig_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("rig.tr2");

            Assert.AreEqual((uint)112, lvl.NumEntities);
        }

        [TestMethod]
        public void DA_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("platform.tr2");

            Assert.AreEqual((uint)131, lvl.NumEntities);
        }

        [TestMethod]
        public void Fathoms_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("unwater.tr2");

            Assert.AreEqual((uint)69, lvl.NumEntities);
        }

        [TestMethod]
        public void Doria_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("keel.tr2");

            Assert.AreEqual((uint)199, lvl.NumEntities);
        }

        [TestMethod]
        public void LQ_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("living.tr2");

            Assert.AreEqual((uint)92, lvl.NumEntities);
        }

        [TestMethod]
        public void Deck_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("deck.tr2");

            Assert.AreEqual((uint)105, lvl.NumEntities);
        }

        [TestMethod]
        public void Tibet_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("skidoo.tr2");

            Assert.AreEqual((uint)108, lvl.NumEntities);
        }

        [TestMethod]
        public void BKang_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("monastry.tr2");

            Assert.AreEqual((uint)217, lvl.NumEntities);
        }

        [TestMethod]
        public void Talion_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("catacomb.tr2");

            Assert.AreEqual((uint)170, lvl.NumEntities);
        }

        [TestMethod]
        public void IcePalace_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("icecave.tr2");

            Assert.AreEqual((uint)158, lvl.NumEntities);
        }

        [TestMethod]
        public void Xian_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("emprtomb.tr2");

            Assert.AreEqual((uint)240, lvl.NumEntities);
        }

        [TestMethod]
        public void Floating_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("floating.tr2");

            Assert.AreEqual((uint)141, lvl.NumEntities);
        }

        [TestMethod]
        public void Lair_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("xian.tr2");

            Assert.AreEqual((uint)66, lvl.NumEntities);
        }

        [TestMethod]
        public void HSH_ReadTest()
        {
            TR2LevelReader reader = new TR2LevelReader();

            TR2Level lvl = reader.ReadLevel("house.tr2");

            Assert.AreEqual((uint)108, lvl.NumEntities);
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
    }
}
