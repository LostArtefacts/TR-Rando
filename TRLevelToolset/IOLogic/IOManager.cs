using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TREnvironmentEditor;
using TRLevelControl;
using TRLevelControl.Model;

namespace TRLevelToolset.IOLogic;

public static class IOManager
{
    public static TRLevelBase? CurrentLevel { get; set; }

    public static string? FileName { get; set; }

    public static TRGame LoadedGame { get; set; }

    public static TR1Level? CurrentLevelAsTR1 
    {
        get
        {
            if (CurrentLevel != null && LoadedGame == TRGame.TR1)
            {
                return CurrentLevel as TR1Level;
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
                TR1LevelControl TR1reader = new();
                CurrentLevel = TR1reader.Read(fname);
                break;
            case TRGame.TR2:
                TR2LevelControl TR2reader = new();
                CurrentLevel = TR2reader.Read(fname);
                break;
            case TRGame.TR3:
                TR3LevelControl TR3reader = new();
                CurrentLevel = TR3reader.Read(fname);
                break;
            case TRGame.TR4:
                TR4LevelControl TR4reader = new();
                CurrentLevel = TR4reader.Read(fname);
                break;
            case TRGame.TR5:
                TR5LevelControl TR5reader = new();
                CurrentLevel = TR5reader.Read(fname);
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
