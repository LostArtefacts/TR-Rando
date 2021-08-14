using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelReader.Compression;
using TRLevelReader.Model;

namespace TRLevelReader
{
    public class TR4LevelReader
    {
        private BinaryReader reader;

        public TR4LevelReader()
        {

        }

        public TR4Level ReadLevel(string Filename)
        {
            if (!Filename.ToUpper().Contains("TR4"))
            {
                throw new NotImplementedException("File reader only supports TR4 levels");
            }

            TR4Level level = new TR4Level();
            reader = new BinaryReader(File.Open(Filename, FileMode.Open));

            //Version
            level.Version = reader.ReadUInt32();
            if (level.Version != Versions.TR45)
            {
                throw new NotImplementedException("File reader only suppors TR4 levels");
            }

            //Texture Counts
            level.NumRoomTextiles = reader.ReadUInt16();
            level.NumObjTextiles = reader.ReadUInt16();
            level.NumBumpTextiles = reader.ReadUInt16();

            //Texture 32 Chunk
            //Get Raw Chunk Data
            level.Texture32Chunk = new TR4Texture32Chunk();
            level.Texture32Chunk.UncompressedSize = reader.ReadUInt32();
            level.Texture32Chunk.CompressedSize = reader.ReadUInt32();
            level.Texture32Chunk.CompressedChunk = reader.ReadBytes((int)level.Texture32Chunk.CompressedSize);

            //Decompress
            DecompressTexture32Chunk(level);

            //Texture 16 Chunk
            //Get Raw Chunk Data
            level.Texture16Chunk = new TR4Texture16Chunk();
            level.Texture16Chunk.UncompressedSize = reader.ReadUInt32();
            level.Texture16Chunk.CompressedSize = reader.ReadUInt32();
            level.Texture16Chunk.CompressedChunk = reader.ReadBytes((int)level.Texture16Chunk.CompressedSize);

            //Decompress
            DecompressTexture16Chunk(level);

            //Sky and Font 32 Chunk
            //Get Raw Chunk Data
            level.SkyAndFont32Chunk = new TR4SkyAndFont32Chunk();
            level.SkyAndFont32Chunk.UncompressedSize = reader.ReadUInt32();
            level.SkyAndFont32Chunk.CompressedSize = reader.ReadUInt32();
            level.SkyAndFont32Chunk.CompressedChunk = reader.ReadBytes((int)level.SkyAndFont32Chunk.CompressedSize);

            //Decompress
            DecompressSkyAndFont32Chunk(level);

            //Level Data Chunk
            //Get Raw Chunk Data
            level.LevelDataChunk = new TR4LevelDataChunk();
            level.LevelDataChunk.UncompressedSize = reader.ReadUInt32();
            level.LevelDataChunk.CompressedSize = reader.ReadUInt32();
            level.LevelDataChunk.CompressedChunk = reader.ReadBytes((int)level.LevelDataChunk.CompressedSize);

            //Decompress
            DecompressLevelDataChunk(level);

            //Samples
            level.NumSamples = reader.ReadUInt32();
            level.Samples = new TR4Sample[level.NumSamples];

            for(int i = 0; i < level.NumSamples; i++)
            {
                TR4Sample sample = new TR4Sample
                {
                    UncompSize = reader.ReadUInt32(),
                    CompSize = reader.ReadUInt32(),
                };

                level.Samples[i] = sample;

                //Compressed chunk is actually NOT zlib compressed - it is simply a WAV file.
                level.Samples[i].CompressedChunk = reader.ReadBytes((int)level.Samples[i].CompSize);
            }

            //We should expect position to be equal to stream length a.k.a all data read.
            Debug.Assert(reader.BaseStream.Position == reader.BaseStream.Length);

            reader.Close();

            return level;
        }

        private void DecompressTexture32Chunk(TR4Level lvl)
        {
            byte[] buffer = TRZlib.Decompress(lvl.Texture32Chunk.CompressedChunk);

            //Is the decompressed chunk the size we expected?
            Debug.Assert(buffer.Length == lvl.Texture32Chunk.UncompressedSize);
        }

        private void DecompressTexture16Chunk(TR4Level lvl)
        {
            byte[] buffer = TRZlib.Decompress(lvl.Texture16Chunk.CompressedChunk);

            //Is the decompressed chunk the size we expected?
            Debug.Assert(buffer.Length == lvl.Texture16Chunk.UncompressedSize);
        }

        private void DecompressSkyAndFont32Chunk(TR4Level lvl)
        {
            byte[] buffer = TRZlib.Decompress(lvl.SkyAndFont32Chunk.CompressedChunk);

            //Is the decompressed chunk the size we expected?
            Debug.Assert(buffer.Length == lvl.SkyAndFont32Chunk.UncompressedSize);
        }

        private void DecompressLevelDataChunk(TR4Level lvl)
        {
            byte[] buffer = TRZlib.Decompress(lvl.LevelDataChunk.CompressedChunk);

            //Is the decompressed chunk the size we expected?
            Debug.Assert(buffer.Length == lvl.LevelDataChunk.UncompressedSize);
        }
    }
}
