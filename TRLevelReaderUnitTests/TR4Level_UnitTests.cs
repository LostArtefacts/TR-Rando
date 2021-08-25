using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using TRLevelReader;
using TRLevelReader.Compression;
using TRLevelReader.Model;

namespace TRLevelReaderUnitTests
{
    [TestClass]
    public class TR4Level_UnitTests
    {
        //TR4 Unit tests are currently performed differently. The compression implemented here
        //seems to give a different byte for byte output after compression compared to the original level file.

        [TestMethod]
        public void alexhub_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("alexhub.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "alexhub_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("alexhub_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("alexhub.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void alexhub2_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("alexhub2.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "alexhub2_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("alexhub2_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("alexhub2.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void ang_race_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("ang_race.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "ang_race_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("ang_race_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("ang_race.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void ankgor1_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("angkor1.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "angkor1_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("angkor1_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("angkor1.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void bikebit_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("bikebit.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "bikebit_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("bikebit_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("bikebit.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void citnew_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("citnew.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "citnew_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("citnew_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("citnew.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void cortyard_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("cortyard.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "cortyard_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("cortyard_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("cortyard.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void csplit1_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("csplit1.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "csplit1_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("csplit1_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("csplit1.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void csplit2_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("csplit2.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "csplit2_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("csplit2_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("csplit2.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void hall_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("hall.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "hall_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("hall_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("hall.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void highstrt_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("highstrt.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "highstrt_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("highstrt_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("highstrt.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void jeepchas_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("jeepchas.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "jeepchas_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("jeepchas_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("jeepchas.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void jeepchs2_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("jeepchs2.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "jeepchs2_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("jeepchs2_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("jeepchs2.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby1a_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby1a.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby1a_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby1a_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby1a.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby1b_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby1b.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby1b_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby1b_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby1b.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby2_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby2.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby2_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby2_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby2.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby3a_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby3a.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby3a_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby3a_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby3a.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby3b_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby3b.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby3b_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby3b_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby3b.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby4a_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby4a.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby4a_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby4a_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby4a.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby4b_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby4b.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby4b_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby4b_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby4b.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby4c_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby4c.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby4c_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby4c_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby4c.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby5a_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby5a.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby5a_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby5a_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby5a.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby5b_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby5b.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby5b_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby5b_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby5b.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby5c_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("joby5c.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "joby5c_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("joby5c_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("joby5c.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void karnak1_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("karnak1.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "karnak1_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("karnak1_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("karnak1.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void lake_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("lake.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "lake_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("lake_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("lake.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void libend_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("libend.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "libend_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("libend_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("libend.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void library_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("library.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "library_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("library_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("library.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void lowstrt_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("lowstrt.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "lowstrt_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("lowstrt_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("lowstrt.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void nutrench_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("nutrench.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "nutrench_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("nutrench_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("nutrench.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void palaces_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("palaces.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "palaces_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("palaces_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("palaces.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void palaces2_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("palaces2.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "palaces2_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("palaces2_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("palaces2.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void semer_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("semer.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "semer_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("semer_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("semer.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void semer2_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("semer2.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "semer2_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("semer2_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("semer2.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void settomb1_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("settomb1.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "settomb1_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("settomb1_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("settomb1.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void settomb2_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("settomb2.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "settomb2_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("settomb2_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("settomb2.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void train_ReadTest()
        {
            //Read original level
            TR4LevelReader reader = new TR4LevelReader();
            TR4Level lvla = reader.ReadLevel("train.tr4");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR4LevelWriter writer = new TR4LevelWriter();
            writer.WriteLevelToFile(lvla, "train_TEST.tr4");

            //Read back the newly written level
            TR4Level lvlb = reader.ReadLevel("train_TEST.tr4");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.LevelDataChunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR4Level lvlc = reader.ReadLevel("train.tr4");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, lvlb.LevelDataChunk.Seperator);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }
    }

    internal class TR45LevelSummary
    {
        public uint LevelChunkUncompressedSize { get; set; }
        public uint Tex32ChunkUncompressedSize { get; set; }
        public uint Tex16ChunkUncompressedSize { get; set; }
        public uint Tex32MChunkUncompressedSize { get; set; }
    }
}
