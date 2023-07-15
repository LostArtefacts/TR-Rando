using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRLevelControl.Compression;
using TRLevelControl.Model;

namespace TRLevelControl
{
    public class TR5LevelReader
    {
        private BinaryReader reader;

        public TR5LevelReader()
        {

        }

        public TR5Level ReadLevel(string Filename)
        {
            if (!Filename.ToUpper().Contains("TRC"))
            {
                throw new NotImplementedException("File reader only supports TR5 levels");
            }

            TR5Level level = new TR5Level();
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

            //TR5 Specific
            level.LaraType = reader.ReadUInt16();
            level.WeatherType = reader.ReadUInt16();
            level.Padding = reader.ReadBytes(28);

            //Level Data Chunk
            //Get Raw Chunk Data
            level.LevelDataChunk = new TR5LevelDataChunk();
            level.LevelDataChunk.UncompressedSize = reader.ReadUInt32();
            level.LevelDataChunk.CompressedSize = reader.ReadUInt32();
            level.LevelDataChunk.CompressedChunk = reader.ReadBytes((int)level.LevelDataChunk.CompressedSize);

            //Decompress
            DecompressLevelDataChunk(level);

            //Samples
            level.NumSamples = reader.ReadUInt32();
            level.Samples = new TR4Sample[level.NumSamples];

            for (int i = 0; i < level.NumSamples; i++)
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

        private void DecompressTexture32Chunk(TR5Level lvl)
        {
            //Decompressed buffer as bytes
            byte[] buffer = TRZlib.Decompress(lvl.Texture32Chunk.CompressedChunk);
            uint[] tiles = new uint[buffer.Length / 4];

            //Convert via block copy to uints
            Buffer.BlockCopy(buffer, 0, tiles, 0, buffer.Length);

            //Is the decompressed chunk the size we expected?
            Debug.Assert(buffer.Length == lvl.Texture32Chunk.UncompressedSize);

            //Allocate expected number of textiles
            lvl.Texture32Chunk.Textiles = new TR4TexImage32[lvl.NumRoomTextiles + lvl.NumObjTextiles + lvl.NumBumpTextiles];

            //Copy from tiles to textile objects
            for (int i = 0; i < lvl.Texture32Chunk.Textiles.Length; i++)
            {
                TR4TexImage32 tex = new TR4TexImage32
                {
                    Tile = new uint[256 * 256]
                };

                //262144 = 256 * 256 * 4
                Buffer.BlockCopy(tiles, (i * 262144), tex.Tile, 0, 262144);

                lvl.Texture32Chunk.Textiles[i] = tex;
            }
        }

        private void DecompressTexture16Chunk(TR5Level lvl)
        {
            //Decompressed buffer as bytes
            byte[] buffer = TRZlib.Decompress(lvl.Texture16Chunk.CompressedChunk);
            ushort[] tiles = new ushort[buffer.Length / 2];

            //Convert via block copy to ushorts
            Buffer.BlockCopy(buffer, 0, tiles, 0, buffer.Length);

            //Is the decompressed chunk the size we expected?
            Debug.Assert(buffer.Length == lvl.Texture16Chunk.UncompressedSize);

            //Allocate expected number of textiles
            lvl.Texture16Chunk.Textiles = new TRTexImage16[lvl.NumRoomTextiles + lvl.NumObjTextiles + lvl.NumBumpTextiles];

            //Copy from tiles to textile objects
            for (int i = 0; i < lvl.Texture16Chunk.Textiles.Length; i++)
            {
                TRTexImage16 tex = new TRTexImage16
                {
                    Pixels = new ushort[256 * 256]
                };

                //131072 = 256 * 256 * 2
                Buffer.BlockCopy(tiles, (i * 131072), tex.Pixels, 0, 131072);

                lvl.Texture16Chunk.Textiles[i] = tex;
            }
        }

        private void DecompressSkyAndFont32Chunk(TR5Level lvl)
        {
            //Decompressed buffer as bytes
            byte[] buffer = TRZlib.Decompress(lvl.SkyAndFont32Chunk.CompressedChunk);
            uint[] tiles = new uint[buffer.Length / 4];

            //Convert via block copy to uints
            Buffer.BlockCopy(buffer, 0, tiles, 0, buffer.Length);

            //Is the decompressed chunk the size we expected?
            Debug.Assert(buffer.Length == lvl.SkyAndFont32Chunk.UncompressedSize);

            //Allocate expected number of textiles
            lvl.SkyAndFont32Chunk.Textiles = new TR4TexImage32[3];

            //Copy from tiles to textile objects
            for (int i = 0; i < lvl.SkyAndFont32Chunk.Textiles.Length; i++)
            {
                TR4TexImage32 tex = new TR4TexImage32
                {
                    Tile = new uint[256 * 256]
                };

                //262144 = 256 * 256 * 4
                Buffer.BlockCopy(tiles, (i * 262144), tex.Tile, 0, 262144);

                lvl.SkyAndFont32Chunk.Textiles[i] = tex;
            }
        }

        private void DecompressLevelDataChunk(TR5Level lvl)
        {
            //TR5 level chunk is not compressed
            byte[] buffer = lvl.LevelDataChunk.CompressedChunk;

            //Is the decompressed chunk the size we expected?
            Debug.Assert(buffer.Length == lvl.LevelDataChunk.UncompressedSize);

            using (MemoryStream stream = new MemoryStream(buffer, false))
            {
                using (BinaryReader lvlChunkReader = new BinaryReader(stream))
                {
                    TR5FileReadUtilities.PopulateRooms(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateFloordata(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateMeshes(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateAnimations(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateMeshTreesFramesModels(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateStaticMeshes(lvlChunkReader, lvl);
                    TR5FileReadUtilities.VerifySPRMarker(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateSprites(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateCameras(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateSoundSources(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateBoxesOverlapsZones(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateAnimatedTextures(lvlChunkReader, lvl);
                    TR5FileReadUtilities.VerifyTEXMarker(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateObjectTextures(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateEntitiesAndAI(lvlChunkReader, lvl);
                    TR5FileReadUtilities.PopulateDemoSoundSampleIndices(lvlChunkReader, lvl);
                    TR5FileReadUtilities.VerifyLevelDataFinalSeperator(lvlChunkReader, lvl);
                }
            }
        }
    }
}
