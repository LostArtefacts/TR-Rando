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
            Assert.AreEqual(OrigSummary.LevelChunkUncompressedSize, lvlb.LevelDataChunk.CompressedChunk.Length);
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
    }
}
