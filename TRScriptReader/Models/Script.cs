using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TRScriptReader.Enums;

namespace TRScriptReader.Models
{
    public class Script
    {
        public uint Version { get; set; }
        public string Description { get; set; }
        public ushort GameflowSize { get; set; }
        public Op<Command> FirstOption { get; set; }
        public Op<Command> TitleReplace { get; set; }
        public Op<Command> OnDeathDemoMode { get; set; }
        public Op<Command> OnDeathInGame { get; set; }
        public uint DemoTime { get; set; }
        public Op<Command> OnDemoInterrupt { get; set; }
        public Op<Command> OnDemoEnd { get; set; }
        public ushort TitleSoundID { get; set; }
        public ushort SingleLevel { get; set; }
        public Flag Flags { get; set; }
        public byte XorKey { get; set; }
        public Language LanguageID { get; set; }
        public ushort SecretSoundID { get; set; }
        public List<Level> Levels { get; set; }
        public List<string> ChapterScreens { get; set; }
        public List<string> Titles { get; set; }
        public List<string> Fmvs { get; set; }
        public List<string> Cutscenes { get; set; }
        public List<ushort> Demos { get; set; }
        //public ushort NumLevels => (ushort) Levels.Count;
        //public ushort NumChapterScreens => (ushort) ChapterScreens.Count;
        //public ushort NumTitles => (ushort) Titles.Count;
        //public ushort NumFmvs => (ushort) Fmvs.Count;
        //public ushort NumCutscenes => (ushort) Cutscenes.Count;
        //public ushort NumDemoLevels => (ushort) Demos.Count;
    }
}
