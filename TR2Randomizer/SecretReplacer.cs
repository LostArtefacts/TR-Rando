using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using Newtonsoft.Json;

namespace TR2Randomizer
{
    public class SecretReplacer
    {
        private List<TRLevel> _TR2LevelList;
        private Random _generator;

        public bool PlaceAll { get; set; }
        
        public SecretReplacer()
        {
            PlaceAll = false;

            _TR2LevelList = new List<TRLevel>
            {
                new TRLevel
                {
                    Name = LevelNames.GW,

                    GoldSecret = new TRItem
                    {
                        ID = 105,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 77,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 21,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.VENICE,

                    GoldSecret = new TRItem
                    {
                        ID = 114,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 100,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 43,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.BARTOLI,

                    GoldSecret = new TRItem
                    {
                        ID = 50,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 125,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 107,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.OPERA,

                    GoldSecret = new TRItem
                    {
                        ID = 44,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 214,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 215,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.RIG,

                    GoldSecret = new TRItem
                    {
                        ID = 108,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 94,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 77,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.DA,

                    GoldSecret = new TRItem
                    {
                        ID = 118,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 5,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 88,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.FATHOMS,

                    GoldSecret = new TRItem
                    {
                        ID = 67,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 42,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 13,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.DORIA,

                    GoldSecret = new TRItem
                    {
                        ID = 6,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 9,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 5,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.LQ,

                    GoldSecret = new TRItem
                    {
                        ID = 77,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 56,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 2,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.DECK,

                    GoldSecret = new TRItem
                    {
                        ID = 90,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 75,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 22,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.TIBET,

                    GoldSecret = new TRItem
                    {
                        ID = 103,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 10,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 3,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.MONASTERY,

                    GoldSecret = new TRItem
                    {
                        ID = 168,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 207,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 21,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.COT,

                    GoldSecret = new TRItem
                    {
                        ID = 87,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 141,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 12,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.CHICKEN,

                    GoldSecret = new TRItem
                    {
                        ID = 142,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 6,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 2,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.XIAN,

                    GoldSecret = new TRItem
                    {
                        ID = 224,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 195,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 30,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.FLOATER,

                    GoldSecret = new TRItem
                    {
                        ID = 38,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 23,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 9,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.LAIR,

                    GoldSecret = new TRItem
                    {
                        ID = 66,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 67,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 68,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },

                new TRLevel
                {
                    Name = LevelNames.HOME,

                    GoldSecret = new TRItem
                    {
                        ID = 108,
                        OID = (int)SecretType.Gold,
                        Params = " 0 -1 -1 0000"
                    },

                    JadeSecret = new TRItem
                    {
                        ID = 109,
                        OID = (int)SecretType.Jade,
                        Params = " 0 -1 -1 0000"
                    },

                    StoneSecret = new TRItem
                    {
                        ID = 110,
                        OID = (int)SecretType.Stone,
                        Params = " 0 -1 -1 0000"
                    }
                },
            };
        }

        private void LaunchTRMod(TRLevel lvl, List<Location> LevelLocations)
        {
            if (LevelLocations.Count > 2)
            {
                ProcessStartInfo trmodLaunch = new ProcessStartInfo
                {
                    FileName = "trmod.exe",
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                if (PlaceAll)
                {
                    PlaceAllSecrets(lvl, LevelLocations);
                    return;
                }

                do
                {
                    lvl.GoldSecret.Location = LevelLocations[_generator.Next(0, LevelLocations.Count)];
                } while (lvl.GoldSecret.Location.Difficulty == Difficulty.Hard && ReplacementStatusManager.AllowHard == false);
                

                do
                {
                    lvl.JadeSecret.Location = LevelLocations[_generator.Next(0, LevelLocations.Count)];
                } while ((lvl.JadeSecret.Location.Room == lvl.GoldSecret.Location.Room) || 
                        (lvl.JadeSecret.Location.Difficulty == Difficulty.Hard && ReplacementStatusManager.AllowHard == false));

                do
                {
                    lvl.StoneSecret.Location = LevelLocations[_generator.Next(0, LevelLocations.Count)];
                } while ((lvl.StoneSecret.Location.Room == lvl.GoldSecret.Location.Room) || 
                        (lvl.StoneSecret.Location.Room == lvl.JadeSecret.Location.Room) ||
                        (lvl.StoneSecret.Location.Difficulty == Difficulty.Hard && ReplacementStatusManager.AllowHard == false));

                if (lvl.DoesLevelHaveSecrets())
                {
                    trmodLaunch.Arguments = lvl.Name + lvl.GoldSecret.TRMODReplaceCommand;
                    var trmod = Process.Start(trmodLaunch);

                    trmod.WaitForExit(1000);

                    trmodLaunch.Arguments = lvl.Name + lvl.JadeSecret.TRMODReplaceCommand;
                    trmod = Process.Start(trmodLaunch);

                    trmod.WaitForExit(1000);

                    trmodLaunch.Arguments = lvl.Name + lvl.StoneSecret.TRMODReplaceCommand;
                    trmod = Process.Start(trmodLaunch);

                    trmod.WaitForExit(1000);
                }
                else
                {
                    trmodLaunch.Arguments = lvl.Name + lvl.GoldSecret.TRMODAddCommand;
                    var trmod = Process.Start(trmodLaunch);

                    trmod.WaitForExit(1000);

                    trmodLaunch.Arguments = lvl.Name + lvl.JadeSecret.TRMODAddCommand;
                    trmod = Process.Start(trmodLaunch);

                    trmod.WaitForExit(1000);

                    trmodLaunch.Arguments = lvl.Name + lvl.StoneSecret.TRMODAddCommand;
                    trmod = Process.Start(trmodLaunch);

                    trmod.WaitForExit(1000);
                }
            }
        }

        private void PlaceAllSecrets(TRLevel lvl, List<Location> LevelLocations)
        {
            ProcessStartInfo trmodLaunch = new ProcessStartInfo
            {
                FileName = "trmod.exe",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            trmodLaunch.Arguments = lvl.Name + " LIST " + lvl.Name + ".trmlist";

            var trmod = Process.Start(trmodLaunch);

            trmod.WaitForExit(1000);

            List<string> FileOutput = new List<string>(File.ReadAllLines(lvl.Name + ".trmlist"));

            File.Delete(lvl.Name + ".trmlist");

            int index = 0;
            bool FirstJade = true;

            foreach (string Item in FileOutput)
            {   
                if (Item.Contains("Item(191"))
                {
                    if (FirstJade == false)
                    {
                        lvl.JadeSecret.Location = LevelLocations[0];
                        trmodLaunch.Arguments = lvl.Name + lvl.JadeSecret.TRMODReplaceCommand;
                        trmod = Process.Start(trmodLaunch);
                        trmod.WaitForExit(1000);
                        index++;
                    }
                    else
                    {
                        // Ignore first Jade
                        FirstJade = false;
                        continue;
                    }
                }     
            }

            for (int i = index; i < LevelLocations.Count; i++)
            {
                lvl.JadeSecret.Location = LevelLocations[i];
                trmodLaunch.Arguments = lvl.Name + lvl.JadeSecret.TRMODAddCommand;
                trmod = Process.Start(trmodLaunch);
                trmod.WaitForExit(1000);
            }
        }

        public void Replace(int seed)
        {
            ReplacementStatusManager.CanRandomize = false;

            _generator = new Random(seed);

            Dictionary<string, List<Location>> Locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText("locations.json"));

            foreach (TRLevel lvl in _TR2LevelList)
            {
                LaunchTRMod(lvl, Locations[lvl.Name]);
                ReplacementStatusManager.LevelProgress++;
            }

            ReplacementStatusManager.LevelProgress = 0;
            ReplacementStatusManager.CanRandomize = true;
        }
    }
}
