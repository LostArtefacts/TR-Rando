using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TREnvironmentEditor;
using TRLevelReader;
using TRLevelReader.Model;

namespace TRLevelToolset.IOLogic
{
    public static class IOManager
    {
        public static BaseTRLevel? CurrentLevel { get; set; }

        public static string? FileName { get; set; }

        public static TRGame LoadedGame { get; set; }

        public static TRLevel? CurrentLevelAsTR1 
        {
            get
            {
                if (CurrentLevel != null && LoadedGame == TRGame.TR1)
                {
                    return CurrentLevel as TRLevel;
                }
                else
                {
                    return null;
                }
            }
        }

        public static TR2Level? CurrentLevelAsTR2 
        {
            get
            {
                if (CurrentLevel != null && LoadedGame == TRGame.TR2)
                {
                    return CurrentLevel as TR2Level;
                }
                else
                {
                    return null;
                }
            }
        }

        public static TR3Level? CurrentLevelAsTR3
        {
            get
            {
                if (CurrentLevel != null && LoadedGame == TRGame.TR3)
                {
                    return CurrentLevel as TR3Level;
                }
                else
                {
                    return null;
                }
            }
        }

        public static TR4Level? CurrentLevelAsTR4
        {
            get
            {
                if (CurrentLevel != null && LoadedGame == TRGame.TR4)
                {
                    return CurrentLevel as TR4Level;
                }
                else
                {
                    return null;
                }
            }
        }

        public static TR5Level? CurrentLevelAsTR5
        {
            get
            {
                if (CurrentLevel != null && LoadedGame == TRGame.TR5)
                {
                    return CurrentLevel as TR5Level;
                }
                else
                {
                    return null;
                }
            }
        }

        public static void Load(string fname, TRGame game)
        {
            FileName = fname;
            LoadedGame = game;

            switch (game)
            {
                case TRGame.TR1:
                    TR1LevelReader TR1reader = new TR1LevelReader();
                    CurrentLevel = TR1reader.ReadLevel(fname);
                    break;
                case TRGame.TR2:
                    TR2LevelReader TR2reader = new TR2LevelReader();
                    CurrentLevel = TR2reader.ReadLevel(fname);
                    break;
                case TRGame.TR3:
                    TR3LevelReader TR3reader = new TR3LevelReader();
                    CurrentLevel = TR3reader.ReadLevel(fname);
                    break;
                case TRGame.TR4:
                    TR4LevelReader TR4reader = new TR4LevelReader();
                    CurrentLevel = TR4reader.ReadLevel(fname);
                    break;
                case TRGame.TR5:
                    TR5LevelReader TR5reader = new TR5LevelReader();
                    CurrentLevel = TR5reader.ReadLevel(fname);
                    break;
                default:
                    break;
            }
        }
    }

    public enum TRGame
    {
        TR1,
        TR2,
        TR3,
        TR4,
        TR5
    }
}
