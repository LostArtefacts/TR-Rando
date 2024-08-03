using Newtonsoft.Json;
using TRImageControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Textures;

namespace TextureExport.Types;

public static class TRRExporter
{
    public static void Export(string ddsFolder, TRGameVersion version)
    {
        string dir = $@"{version}\DDS\PNG";
        Directory.CreateDirectory(dir);

        foreach (string ddsFile in Directory.GetFiles(ddsFolder))
        {
            TRImage image = new(ddsFile);
            image.Save(Path.Combine(dir, Path.ChangeExtension(Path.GetFileName(ddsFile), ".png")));
        }
    }

    public static void GenerateCategories(TRGameVersion version)
    {
        Dictionary<TRArea, List<string>> gameAreas;
        Dictionary<TRTexCategory, SortedSet<ushort>> categories = new();
        Dictionary<TRTexCategory, ushort> defaults = new();
        SortedSet<ushort> animated;

        foreach (TRTexCategory category in Enum.GetValues<TRTexCategory>())
        {
            string dir = $@"{version}\DDS\Categories\{category}";
            if (!Directory.Exists(dir))
            {
                continue;
            }

            categories[category] = new();
            foreach (string img in Directory.GetFiles(dir))
            {
                if (ushort.TryParse(Path.GetFileNameWithoutExtension(img), out ushort id))
                {
                    categories[category].Add(id);
                }
            }
        }

        void WriteInfo<T>(List<Dictionary<string, Dictionary<T, TRItemFlags>>> itemFlags)
            where T : Enum
        {
            TRTexInfo<T> info = new()
            {
                GameAreas = gameAreas,
                Categories = categories,
                Animated = animated,
                Defaults = defaults,
                ItemFlags = itemFlags
            };

            File.WriteAllText(Path.GetFullPath($@"..\..\..\TRRandomizerCore\Resources\{version}\Textures\texinfo.json"), JsonConvert.SerializeObject(info));
        }

        switch (version)
        {
            case TRGameVersion.TR1:
                gameAreas = new()
                {
                    [TRArea.Home] = new() { TR1LevelNames.ASSAULT },
                    [TRArea.Peru] = TR1LevelNames.Peru,
                    [TRArea.Greece] = new() { TR1LevelNames.FOLLY, TR1LevelNames.COLOSSEUM, TR1LevelNames.MIDAS },
                    [TRArea.Cistern] = new() { TR1LevelNames.CISTERN, TR1LevelNames.TIHOCAN },
                    [TRArea.Egypt] = TR1LevelNames.Egypt,
                    [TRArea.Atlantis] = TR1LevelNames.Atlantis,
                };
                defaults[TRTexCategory.Transparent] = 477;
                defaults[TRTexCategory.Lever] = 34;
                animated = new() { 554, 558, 571, 590, 651, 652, 654, 655 };
                WriteInfo(_tr1ItemFlags);
                break;

            case TRGameVersion.TR2:
                gameAreas = new()
                {
                    [TRArea.Home] = new() { TR2LevelNames.ASSAULT, TR2LevelNames.HOME },
                    [TRArea.GreatWall] = TR2LevelNames.GreatWall,
                    [TRArea.Italy] = TR2LevelNames.Italy,
                    [TRArea.Offshore] = new() { TR2LevelNames.RIG, TR2LevelNames.DA },
                    [TRArea.Underwater] = new() { TR2LevelNames.FATHOMS, TR2LevelNames.DORIA, TR2LevelNames.LQ, TR2LevelNames.DECK },
                    [TRArea.Tibet] = TR2LevelNames.Tibet,
                    [TRArea.Xian] = TR2LevelNames.China,
                };
                defaults[TRTexCategory.Transparent] = 159;
                defaults[TRTexCategory.Lever] = 1202;
                defaults[TRTexCategory.Ladder] = 1172;
                animated = new() { 375, 401, 402, 403, 413, 414, 415, 416, 654, 721, 722 };
                WriteInfo(_tr2ItemFlags);
                break;

            case TRGameVersion.TR3:
                gameAreas = new()
                {
                    [TRArea.Home] = new() { TR3LevelNames.ASSAULT },
                    [TRArea.India] = TR3LevelNames.India,
                    [TRArea.SouthPacific] = TR3LevelNames.SouthPacific,
                    [TRArea.London] = TR3LevelNames.London,
                    [TRArea.Nevada] = TR3LevelNames.Nevada,
                    [TRArea.Antarctica] = TR3LevelNames.Antarctica,
                };
                defaults[TRTexCategory.Transparent] = 794;
                defaults[TRTexCategory.Lever] = 1002;
                defaults[TRTexCategory.Ladder] = 600;
                animated = new() { 374, 375, 376, 377, 378, 379, 380, 381, 806, 807, 808, 809, 818, 819, 820, 821 };
                WriteInfo(_tr3ItemFlags);
                break;
        }
    }

    private static readonly List<Dictionary<string, Dictionary<TR1Type, TRItemFlags>>> _tr1ItemFlags = new()
    {
        new()
        {
            [TR1LevelNames.ASSAULT] = new(),
        },
        new()
        {
            [TR1LevelNames.CAVES] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.Door3] = TRItemFlags.RightDoor | TRItemFlags.EightClick,
                [TR1Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.DartEmitter] = TRItemFlags.Darts | TRItemFlags.PairA,
                [TR1Type.Dart_H] = TRItemFlags.Darts | TRItemFlags.PairB,
            },
            [TR1LevelNames.VILCABAMBA] = new()
            {
                // Door3 omitted as its mesh vertices aren't regular
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.Door6] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR1Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR1Type.DartEmitter] = TRItemFlags.Darts | TRItemFlags.PairA,
                [TR1Type.Dart_H] = TRItemFlags.Darts | TRItemFlags.PairB,
            },
            [TR1LevelNames.VALLEY] = new()
            {
                // Door6 omitted as it appears to be a dummy with a static mesh in its place
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
            [TR1LevelNames.QUALOPEC] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR1Type.Door2] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR1Type.Door3] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR1Type.Door4] = TRItemFlags.Drawbridge | TRItemFlags.SixClick,
                [TR1Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door6] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door8] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.RollingBall] = TRItemFlags.Boulder,
            },
        },
        new()
        {
            [TR1LevelNames.FOLLY] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door2] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR1Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.EightClick,
                [TR1Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.Door6] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.Door7] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR1Type.RollingBall] = TRItemFlags.Boulder,
            },
            [TR1LevelNames.COLOSSEUM] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR1Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.EightClick,
                [TR1Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door6] = TRItemFlags.RightDoor | TRItemFlags.EightClick,
                [TR1Type.Door7] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR1Type.Door8] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.RollingBall] = TRItemFlags.Boulder,
            },
            [TR1LevelNames.MIDAS] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door6] = TRItemFlags.RightDoor | TRItemFlags.EightClick,
                [TR1Type.Door7] = TRItemFlags.LeftDoor | TRItemFlags.EightClick,
                [TR1Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.PushBlock2] = TRItemFlags.PushBlock,
                [TR1Type.SlammingDoor] = TRItemFlags.SlammingDoor,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
            [TR1LevelNames.CISTERN] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR1Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.PushBlock2] = TRItemFlags.PushBlock,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
            },
            [TR1LevelNames.TIHOCAN] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR1Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.EightClick,
                [TR1Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.SlammingDoor] = TRItemFlags.SlammingDoor,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR1Type.DartEmitter] = TRItemFlags.Darts | TRItemFlags.PairA,
                [TR1Type.Dart_H] = TRItemFlags.Darts | TRItemFlags.PairB,
            },
        },
        new()
        {
            [TR1LevelNames.KHAMOON] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick | TRItemFlags.PairA,
                [TR1Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door3] = TRItemFlags.RightDoor | TRItemFlags.FourClick | TRItemFlags.PairA,
                [TR1Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairB,
                [TR1Type.Door5] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairB,
                [TR1Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR1Type.RollingBall] = TRItemFlags.Boulder,
            },
            [TR1LevelNames.OBELISK] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.Door6] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
            [TR1LevelNames.SANCTUARY] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.EightClick,
                [TR1Type.Door4] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR1Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.Door6] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.SlammingDoor] = TRItemFlags.SlammingDoor,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
            }
        },
        new()
        {
            [TR1LevelNames.MINES] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.Door4] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR1Type.Trapdoor2] = TRItemFlags.Trapdoor,
                [TR1Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.PushBlock2] = TRItemFlags.PushBlock,
                [TR1Type.PushBlock3] = TRItemFlags.PushBlock,
                [TR1Type.PushBlock4] = TRItemFlags.PushBlock,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.RollingBall] = TRItemFlags.Boulder,
            },
            [TR1LevelNames.ATLANTIS] = new()
            {
                [TR1Type.Door3] = TRItemFlags.RightDoor | TRItemFlags.EightClick,
                [TR1Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door6] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door7] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door8] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Trapdoor2] = TRItemFlags.Trapdoor,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.SlammingDoor] = TRItemFlags.SlammingDoor,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR1Type.RollingBall] = TRItemFlags.Boulder,
                [TR1Type.DartEmitter] = TRItemFlags.Darts | TRItemFlags.PairA,
                [TR1Type.Dart_H] = TRItemFlags.Darts | TRItemFlags.PairB,
            },
            [TR1LevelNames.PYRAMID] = new()
            {
                [TR1Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR1Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR1Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR1Type.SlammingDoor] = TRItemFlags.SlammingDoor,
                [TR1Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR1Type.RollingBall] = TRItemFlags.Boulder,
                [TR1Type.DartEmitter] = TRItemFlags.Darts | TRItemFlags.PairA,
                [TR1Type.Dart_H] = TRItemFlags.Darts | TRItemFlags.PairB,
            }
        },
    };

    private static readonly List<Dictionary<string, Dictionary<TR2Type, TRItemFlags>>> _tr2ItemFlags = new()
    {
        new()
        {
            [TR2LevelNames.ASSAULT] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR2Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR2Type.BreakableWindow1] = TRItemFlags.BreakableWindow,
                [TR2Type.PushButtonSwitch] = TRItemFlags.PushButton,
            },
            [TR2LevelNames.HOME] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR2Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR2Type.BreakableWindow1] = TRItemFlags.BreakableWindow,
                [TR2Type.PushButtonSwitch] = TRItemFlags.PushButton,
            },
        },
        new()
        {
            [TR2LevelNames.GW] = new()
            {
                [TR2Type.Door1] = TRItemFlags.RightDoor | TRItemFlags.FiveClick,
                [TR2Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.RollingBall] = TRItemFlags.Boulder | TRItemFlags.PairA,
                [TR2Type.TeethSpikesOrGlassShards] = TRItemFlags.Spikes,
                [TR2Type.RollingSpindle] = TRItemFlags.Spindle,
            },
            [TR2LevelNames.VENICE] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairB,
                [TR2Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.LiftingDoor2] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairB,
                [TR2Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairC,
                [TR2Type.LiftingDoor1] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairC,
                [TR2Type.LiftingDoor3] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.BreakableWindow1] = TRItemFlags.BreakableWindow,
                [TR2Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
            },
            [TR2LevelNames.BARTOLI] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairB,
                [TR2Type.LiftingDoor1] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairB,
                [TR2Type.LiftingDoor3] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.BreakableWindow1] = TRItemFlags.BreakableWindow,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.PushBlock2] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.Trapdoor2] = TRItemFlags.Trapdoor,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
            },
            [TR2LevelNames.OPERA] = new()
            {
                [TR2Type.Door1] = TRItemFlags.RightDoor | TRItemFlags.SixClick | TRItemFlags.PairA,
                [TR2Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.SixClick | TRItemFlags.PairA,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door4] = TRItemFlags.RightDoor | TRItemFlags.EightClick,
                [TR2Type.Door5] = TRItemFlags.RightDoor | TRItemFlags.FiveClick,
                [TR2Type.LiftingDoor1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.LiftingDoor2] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR2Type.LiftingDoor3] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.BreakableWindow1] = TRItemFlags.BreakableWindow,
                [TR2Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR2Type.BouldersOrSnowballs] = TRItemFlags.Boulder | TRItemFlags.PairB,
                [TR2Type.TeethSpikesOrGlassShards] = TRItemFlags.Spikes,
            },
        },
        new()
        {
            [TR2LevelNames.RIG] = new()
            {
                // Door5 omitted as split; Door6 omitted as wheel door
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.BreakableWindow1] = TRItemFlags.BreakableWindow,
                [TR2Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.Trapdoor2] = TRItemFlags.Trapdoor,
                [TR2Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR2Type.RollingStorageDrums] = TRItemFlags.Boulder | TRItemFlags.PairC,
            },
            [TR2LevelNames.DA] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.Trapdoor2] = TRItemFlags.Trapdoor,
                [TR2Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
            },
        },
        new()
        {
            [TR2LevelNames.FATHOMS] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
            [TR2LevelNames.DORIA] = new()
            {
                // Door2 omitted (wheel door)
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door3] = TRItemFlags.RightDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR2Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR2Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.LiftingDoor1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR2Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR2Type.PushBlock2] = TRItemFlags.PushBlock,
                [TR2Type.PushBlock3] = TRItemFlags.PushBlock,
                [TR2Type.PushBlock4] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.Trapdoor2] = TRItemFlags.Trapdoor,
                [TR2Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.RollingStorageDrums] = TRItemFlags.Boulder | TRItemFlags.PairC,
                [TR2Type.TeethSpikesOrGlassShards] = TRItemFlags.Spikes,
            },
            [TR2LevelNames.LQ] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR2Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR2Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.PushBlock2] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.RollingStorageDrums] = TRItemFlags.Boulder | TRItemFlags.PairC,
                [TR2Type.TeethSpikesOrGlassShards] = TRItemFlags.Spikes,
            },
            [TR2LevelNames.DECK] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR2Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR2Type.PushBlock2] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.TeethSpikesOrGlassShards] = TRItemFlags.Spikes,
            },
        },
        new()
        {
            [TR2LevelNames.TIBET] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.BreakableWindow1] = TRItemFlags.BreakableWindow,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor | TRItemFlags.EightClick,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.BouldersOrSnowballs] = TRItemFlags.Boulder | TRItemFlags.PairB,
            },
            [TR2LevelNames.MONASTERY] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.BreakableWindow1] = TRItemFlags.BreakableWindow,
                [TR2Type.BreakableWindow2] = TRItemFlags.BreakableWindow,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.RollingBall] = TRItemFlags.Boulder | TRItemFlags.PairA,
                [TR2Type.RollingSpindle] = TRItemFlags.Spindle,
            },
            [TR2LevelNames.COT] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.LiftingDoor3] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR2Type.PushBlock2] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor2] = TRItemFlags.Trapdoor | TRItemFlags.EightClick,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.BouldersOrSnowballs] = TRItemFlags.Boulder | TRItemFlags.PairB,
                [TR2Type.TeethSpikesOrGlassShards] = TRItemFlags.Spikes,
            },
            [TR2LevelNames.CHICKEN] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.LiftingDoor3] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.BreakableWindow2] = TRItemFlags.BreakableWindow,
                [TR2Type.BouncePad] = TRItemFlags.Springboard,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.BouldersOrSnowballs] = TRItemFlags.Boulder | TRItemFlags.PairB,
                [TR2Type.TeethSpikesOrGlassShards] = TRItemFlags.Spikes,
            },
        },
        new()
        {
            [TR2LevelNames.XIAN] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairB,
                [TR2Type.Door5] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairB,
                [TR2Type.LiftingDoor1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR2Type.LiftingDoor2] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR2Type.LiftingDoor3] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR2Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR2Type.BouncePad] = TRItemFlags.Springboard,
                [TR2Type.PushBlock2] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR2Type.Trapdoor2] = TRItemFlags.Trapdoor | TRItemFlags.EightClick,
                [TR2Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.RollingBall] = TRItemFlags.Boulder | TRItemFlags.PairA,
                [TR2Type.TeethSpikesOrGlassShards] = TRItemFlags.Spikes,
                [TR2Type.RollingSpindle] = TRItemFlags.Spindle,
            },
            [TR2LevelNames.FLOATER] = new()
            {
                [TR2Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairB,
                [TR2Type.Door4] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairB,
                [TR2Type.LiftingDoor1] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.LiftingDoor2] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.LiftingDoor3] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.PushBlock1] = TRItemFlags.PushBlock,
                [TR2Type.Trapdoor1] = TRItemFlags.Trapdoor | TRItemFlags.EightClick,
                [TR2Type.Trapdoor2] = TRItemFlags.Trapdoor,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
                [TR2Type.RollingBall] = TRItemFlags.Boulder | TRItemFlags.PairA,
                [TR2Type.TeethSpikesOrGlassShards] = TRItemFlags.Spikes,
            },
            [TR2LevelNames.LAIR] = new()
            {
                [TR2Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door4] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR2Type.Door5] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.LiftingDoor1] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR2Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
        }
    };

    private static readonly List<Dictionary<string, Dictionary<TR3Type, TRItemFlags>>> _tr3ItemFlags = new()
    {
        new()
        {
            [TR3LevelNames.ASSAULT] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR3Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR3Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR3Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR3Type.Door6] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR3Type.Door7] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick,
                [TR3Type.Door8] = TRItemFlags.LeftDoor | TRItemFlags.EightClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
            },
        },
        new()
        {
            [TR3LevelNames.JUNGLE] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door8] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
            [TR3LevelNames.RUINS] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR3Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door3] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR3Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
            [TR3LevelNames.GANGES] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR3Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door3] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
            },
            [TR3LevelNames.CAVES] = new()
            {
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
            },
        },
        new()
        {
            [TR3LevelNames.COASTAL] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick,
                [TR3Type.Door4] = TRItemFlags.LeftDoor| TRItemFlags.FourClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
            },
            [TR3LevelNames.CRASH] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.SixClick,
                [TR3Type.Door2] = TRItemFlags.LeftDoor| TRItemFlags.FourClick,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.Trapdoor2] = TRItemFlags.Trapdoor,
                [TR3Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR3Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
            [TR3LevelNames.MADUBU] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
            },
            [TR3LevelNames.PUNA] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR3Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR3Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
                [TR3Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
        },
        new()
        {
            [TR3LevelNames.THAMES] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
                [TR3Type.PushableBlock2] = TRItemFlags.PushBlock,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.Trapdoor2] = TRItemFlags.Trapdoor,
                [TR3Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
            },
            [TR3LevelNames.ALDWYCH] = new()
            {
                // Door1 is mini-door with a static mesh, so ignored
                [TR3Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door6] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door7] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.Trapdoor2] = TRItemFlags.Trapdoor,
            },
            [TR3LevelNames.LUDS] = new()
            {
                [TR3Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door7] = TRItemFlags.RightDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR3Type.Door8] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR3Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
                [TR3Type.PushableBlock2] = TRItemFlags.PushBlock,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
            },
            [TR3LevelNames.CITY] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
            },
            [TR3LevelNames.HALLOWS] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.FallingBlock] = TRItemFlags.FallingBlock,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
        },
        new()
        {
            [TR3LevelNames.NEVADA] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR3Type.Door3] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR3Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
            },
            [TR3LevelNames.HSC] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR3Type.Door3] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR3Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door5] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR3Type.Door6] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
            },
            [TR3LevelNames.AREA51] = new()
            {
                // Doors 7 and 8 are non-standard so ignored
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door4] = TRItemFlags.RightDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR3Type.Door5] = TRItemFlags.LeftDoor | TRItemFlags.FiveClick | TRItemFlags.PairA,
                [TR3Type.Door6] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.Trapdoor2] = TRItemFlags.Trapdoor,
            },
        },
        new()
        {
            [TR3LevelNames.ANTARC] = new()
            {
                // Doors 1, 2 and 5 are mini-doors; door6 is a gauge
                [TR3Type.Door3] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR3Type.Door4] = TRItemFlags.LeftDoor | TRItemFlags.SixClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
            [TR3LevelNames.RXTECH] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door3] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
            },
            [TR3LevelNames.TINNOS] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR3Type.Door2] = TRItemFlags.RightDoor | TRItemFlags.EightClick | TRItemFlags.PairA,
                [TR3Type.Door3] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR3Type.Door4] = TRItemFlags.LiftingDoor | TRItemFlags.EightClick,
                [TR3Type.Door6] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door7] = TRItemFlags.LiftingDoor | TRItemFlags.FourClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
                [TR3Type.PushableBlock1] = TRItemFlags.PushBlock,
                [TR3Type.Trapdoor1] = TRItemFlags.Trapdoor,
                [TR3Type.UnderwaterSwitch] = TRItemFlags.UnderwaterSwitch,
                [TR3Type.WallSwitch] = TRItemFlags.WallSwitch,
            },
            [TR3LevelNames.WILLIE] = new()
            {
                [TR3Type.Door1] = TRItemFlags.LeftDoor | TRItemFlags.FourClick,
                [TR3Type.Door2] = TRItemFlags.LeftDoor | TRItemFlags.SixClick,
                [TR3Type.PushButtonSwitch] = TRItemFlags.PushButton,
            },
        },
    };
}
