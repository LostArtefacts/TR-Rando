using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TRLevelReader;
using TRLevelReader.Compression;
using TRLevelReader.Model;

namespace TRLevelReaderUnitTests
{
    [TestClass]
    public class TR5Level_UnitTests
    {
        [TestMethod]
        public void Andrea1_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("andrea1.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "andrea1_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("andrea1_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("andrea1.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void Andrea2_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("andrea2.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "andrea2_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("andrea2_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("andrea2.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void Andrea3_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("andrea3.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "andrea3_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("andrea3_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("andrea3.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void andy1_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("andy1.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "andy1_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("andy1_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("andy1.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void Andy2_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("andy2.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "andy2_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("andy2_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("andy2.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void andy3_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("andy3.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "andy3_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("andy3_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("andy3.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby2_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("joby2.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "joby2_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("joby2_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("joby2.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby3_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("joby3.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "joby3_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("joby3_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("joby3.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby4_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("joby4.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "joby4_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("joby4_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("joby4.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void joby5_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("joby5.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "joby5_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("joby5_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("joby5.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void rich1_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("rich1.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "rich1_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("rich1_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("rich1.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void rich2_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("rich2.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "rich2_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("rich2_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("rich2.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void rich3_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("rich3.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "rich3_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("rich3_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("rich3.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }

        [TestMethod]
        public void richcut2_ReadTest()
        {
            //Read original level
            TR5LevelReader reader = new TR5LevelReader();
            TR5Level lvla = reader.ReadLevel("richcut2.trc");

            //What are the original uncompressed data sizes?
            TR45LevelSummary OrigSummary = new TR45LevelSummary
            {
                LevelChunkUncompressedSize = lvla.LevelDataChunk.UncompressedSize,
                Tex32ChunkUncompressedSize = lvla.Texture32Chunk.UncompressedSize,
                Tex16ChunkUncompressedSize = lvla.Texture16Chunk.UncompressedSize,
                Tex32MChunkUncompressedSize = lvla.SkyAndFont32Chunk.UncompressedSize
            };

            //Serialize to new file - this will also recalculate chunk sizes
            TR5LevelWriter writer = new TR5LevelWriter();
            writer.WriteLevelToFile(lvla, "richcut2_TEST.trc");

            //Read back the newly written level
            TR5Level lvlb = reader.ReadLevel("richcut2_TEST.trc");

            //Verify - have we lost any data?
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, (uint)lvlb.LevelDataChunk.CompressedChunk.Length);
            Assert.AreEqual(OrigSummary.Tex32ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex16ChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk).Length);
            Assert.AreEqual(OrigSummary.Tex32MChunkUncompressedSize, (uint)TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk).Length);

            //Read the original again
            TR5Level lvlc = reader.ReadLevel("richcut2.trc");

            //Verify - is the real data still byte for byte with the original?
            //So if we decompress the original level file chunks, and the level file written by this library, do they produce the same output?
            CollectionAssert.AreEqual(lvlc.LevelDataChunk.CompressedChunk, lvlb.LevelDataChunk.CompressedChunk);
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture32Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.Texture16Chunk.CompressedChunk), TRZlib.Decompress(lvlb.Texture16Chunk.CompressedChunk));
            CollectionAssert.AreEqual(TRZlib.Decompress(lvlc.SkyAndFont32Chunk.CompressedChunk), TRZlib.Decompress(lvlb.SkyAndFont32Chunk.CompressedChunk));
        }
    }
}
