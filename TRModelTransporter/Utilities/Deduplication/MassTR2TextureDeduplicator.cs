using System.Collections.Generic;
using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl.Model.Enums;
using TRModelTransporter.Model.Textures;
using TRModelTransporter.Packing;

namespace TRModelTransporter.Utilities
{
    public class MassTR2TextureDeduplicator : AbstractMassTRTextureDeduplicator<TR2Entities, TR2Level>
    {
        public override List<string> LevelNames => TR2LevelNames.AsList;

        private readonly TR2LevelControl _reader;
        private readonly TR2LevelWriter _writer;

        public MassTR2TextureDeduplicator()
        {
            _reader = new TR2LevelControl();
            _writer = new TR2LevelWriter();
        }

        protected override AbstractTexturePacker<TR2Entities, TR2Level> CreatePacker(TR2Level level)
        {
            return new TR2TexturePacker(level);
        }

        protected override AbstractTextureRemapGroup<TR2Entities, TR2Level> CreateRemapGroup()
        {
            return new TR2TextureRemapGroup();
        }

        protected override AbstractTRLevelTextureDeduplicator<TR2Entities, TR2Level> CreateDeduplicator()
        {
            return new TR2LevelTextureDeduplicator();
        }

        protected override TR2Level ReadLevel(string path)
        {
            return _reader.Read(path);
        }

        protected override void WriteLevel(TR2Level level, string path)
        {
            _writer.WriteLevelToFile(level, path);
        }
    }
}