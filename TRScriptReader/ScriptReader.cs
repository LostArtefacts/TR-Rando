using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRScriptReader.Enums;
using TRScriptReader.Models;

namespace TRScriptReader
{
    public class ScriptReader
    {
        public ScriptReader()
        {

        }

        private Op<T> GetOp<T>(int value) where T: Enum
        {
            return new Op<T>
            {
                Type = (T) (object) (value - (value % 0x100)),
                Arg = (byte?) ((value % 0x100) & 0xFF),
            };
        }

        private List<string> ReadStrings(BinaryReader reader, ushort count)
        {
            List<string> strings = new List<string>();
            ushort[] offsets = new ushort[count];
            ushort total = 0;

            for (int i = 0; i < count; ++i)
                offsets[i] = reader.ReadUInt16();
            total = reader.ReadUInt16();
            for (int i = 0; i < count; ++i)
            {
                ushort lower = offsets[i];
                ushort higher = i + 1 == count ? total : offsets[i + 1];
                int bytes = higher - lower;
                strings.Add(Encoding.ASCII.GetString(reader.ReadBytes(bytes)));
            }
            return strings;
        }

        public Script Read(string filePath)
        {
            var script = new Script();

            using (var file = File.Open(filePath, FileMode.Open))
            {
                using (var reader = new BinaryReader(file))
                {
                    script.Version = reader.ReadUInt32();
                    script.Description = Encoding.ASCII.GetString(reader.ReadBytes(256));
                    script.GameflowSize = reader.ReadUInt16();
                    script.FirstOption = GetOp<Command>(reader.ReadInt32());
                    script.TitleReplace = GetOp<Command>(reader.ReadInt32());
                    script.OnDeathDemoMode = GetOp<Command>(reader.ReadInt32());
                    script.OnDeathInGame = GetOp<Command>(reader.ReadInt32());
                    script.DemoTime = reader.ReadUInt32();
                    script.OnDemoInterrupt = GetOp<Command>(reader.ReadInt32());
                    script.OnDemoEnd = GetOp<Command>(reader.ReadInt32());
                    reader.ReadBytes(36);
                    ushort numLevels = reader.ReadUInt16();
                    ushort numChapterScreens = reader.ReadUInt16();
                    ushort numTitles = reader.ReadUInt16();
                    ushort numFmvs = reader.ReadUInt16();
                    ushort numCutscenes = reader.ReadUInt16();
                    ushort numDemoLevels = reader.ReadUInt16();
                    script.TitleSoundID = reader.ReadUInt16();
                    script.SingleLevel = reader.ReadUInt16();
                    reader.ReadBytes(32);
                    script.Flags = (Flag) reader.ReadUInt16();
                    reader.ReadBytes(6);
                    script.XorKey = reader.ReadByte();
                    script.LanguageID = (Language) reader.ReadByte();
                    script.SecretSoundID = reader.ReadUInt16();
                    reader.ReadBytes(4);
                    var levels = ReadStrings(reader, numLevels);
                    script.ChapterScreens = ReadStrings(reader, numChapterScreens);
                    script.Titles = ReadStrings(reader, numTitles);
                    script.Fmvs = ReadStrings(reader, numFmvs);
                    var paths = ReadStrings(reader, numLevels);
                    script.Cutscenes = ReadStrings(reader, numCutscenes);
                }
            }
            return script;
        }
    }
}
