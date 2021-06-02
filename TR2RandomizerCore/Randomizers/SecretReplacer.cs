using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TRLevelReader.Model;
using Newtonsoft.Json;
using TRLevelReader.Model.Enums;
using TR2RandomizerCore.Utilities;
using TR2RandomizerCore.Zones;
using TR2RandomizerCore.Helpers;
using TRGE.Core;

namespace TR2RandomizerCore.Randomizers
{
    public class SecretReplacer : RandomizerBase
    {
        public bool AllowHard { get; set; }
        public bool AllowGlitched { get; set; }
        public bool IsDevelopmentModeOn { get; set; }
        
        public SecretReplacer() : base()
        {
        }

        private void RandomizeSecrets(List<Location> LevelLocations)
        {
            if (LevelLocations.Count > 2)
            {
                if (IsDevelopmentModeOn)
                {
                    PlaceAllSecrets(LevelLocations);
                    return;
                }

                //Apply zoning to the locations to ensure they are spread out.
                ZonedLocationCollection ZonedLocations = new ZonedLocationCollection();

                ZonedLocations.PopulateZones(_levelInstance.Name, LevelLocations, ZonePopulationMethod.SecretsOnly);

                Location GoldSecret;
                Location JadeSecret;
                Location StoneSecret;

                //Find suitable locations, ensuring they are zoned, do not share a room and difficulty.
                //Location = ZoneLocations[ZoneGroup][LocationInZoneGroup]
                do
                {
                    GoldSecret = ZonedLocations.GoldZone[_generator.Next(0, ZonedLocations.GoldZone.Count)];
                } while ((GoldSecret.Difficulty == Difficulty.Hard && AllowHard == false) ||
                        (GoldSecret.RequiresGlitch == true && AllowGlitched == false));
                

                do
                {
                    JadeSecret = ZonedLocations.JadeZone[_generator.Next(0, ZonedLocations.JadeZone.Count)];
                } while ((JadeSecret.Room == GoldSecret.Room) || 
                        (JadeSecret.Difficulty == Difficulty.Hard && AllowHard == false) ||
                        (JadeSecret.RequiresGlitch == true && AllowGlitched == false));

                do
                {
                    StoneSecret = ZonedLocations.StoneZone[_generator.Next(0, ZonedLocations.StoneZone.Count)];
                } while ((StoneSecret.Room == GoldSecret.Room) || 
                        (StoneSecret.Room == JadeSecret.Room) ||
                        (StoneSecret.Difficulty == Difficulty.Hard && AllowHard == false) ||
                        (StoneSecret.RequiresGlitch == true && AllowGlitched == false));

                //Due to TRMod only accepting room space coords entities are actually stored in level space. So include some
                //calls to support a transformation of any locations that are specified in room space to maintain backwards compatbility
                //with older locations and support locations that are specified in both level or room space.
                GoldSecret = SpatialConverters.TransformToLevelSpace(GoldSecret, _levelInstance.Data.Rooms[GoldSecret.Room].Info);
                JadeSecret = SpatialConverters.TransformToLevelSpace(JadeSecret, _levelInstance.Data.Rooms[JadeSecret.Room].Info);
                StoneSecret = SpatialConverters.TransformToLevelSpace(StoneSecret, _levelInstance.Data.Rooms[StoneSecret.Room].Info);

                //Does the level contain the entities?
                int GoldIndex = Array.FindIndex(_levelInstance.Data.Entities, ent => (ent.TypeID == (short)TR2Entities.GoldSecret_S_P));
                int JadeIndex = Array.FindIndex(_levelInstance.Data.Entities, ent => (ent.TypeID == (short)TR2Entities.JadeSecret_S_P));
                int StoneIndex = Array.FindIndex(_levelInstance.Data.Entities, ent => (ent.TypeID == (short)TR2Entities.StoneSecret_S_P));

                //Check if we could find instances of the secret entities, if not, we need to add not edit.
                if (GoldIndex != -1)
                {
                    _levelInstance.Data.Entities[GoldIndex].Room = Convert.ToInt16(GoldSecret.Room);
                    _levelInstance.Data.Entities[GoldIndex].X = GoldSecret.X;
                    _levelInstance.Data.Entities[GoldIndex].Y = GoldSecret.Y;
                    _levelInstance.Data.Entities[GoldIndex].Z = GoldSecret.Z;
                }
                else
                {
                    TR2Entity GoldEntity = new TR2Entity
                    {
                        TypeID = (int)TR2Entities.GoldSecret_S_P,
                        Room = Convert.ToInt16(GoldSecret.Room),
                        X = GoldSecret.X,
                        Y = GoldSecret.Y,
                        Z = GoldSecret.Z,
                        Angle = 0,
                        Intensity1 = -1,
                        Intensity2 = -1,
                        Flags = 0
                    };

                    List<TR2Entity> ents = _levelInstance.Data.Entities.ToList();
                    ents.Add(GoldEntity);
                    _levelInstance.Data.Entities = ents.ToArray();
                    _levelInstance.Data.NumEntities++;
                }

                if (JadeIndex != -1)
                {
                    _levelInstance.Data.Entities[JadeIndex].Room = Convert.ToInt16(JadeSecret.Room);
                    _levelInstance.Data.Entities[JadeIndex].X = JadeSecret.X;
                    _levelInstance.Data.Entities[JadeIndex].Y = JadeSecret.Y;
                    _levelInstance.Data.Entities[JadeIndex].Z = JadeSecret.Z;
                }
                else
                {
                    TR2Entity JadeEntity = new TR2Entity
                    {
                        TypeID = (int)TR2Entities.JadeSecret_S_P,
                        Room = Convert.ToInt16(JadeSecret.Room),
                        X = JadeSecret.X,
                        Y = JadeSecret.Y,
                        Z = JadeSecret.Z,
                        Angle = 0,
                        Intensity1 = -1,
                        Intensity2 = -1,
                        Flags = 0
                    };

                    List<TR2Entity> ents = _levelInstance.Data.Entities.ToList();
                    ents.Add(JadeEntity);
                    _levelInstance.Data.Entities = ents.ToArray();
                    _levelInstance.Data.NumEntities++;
                }

                if (StoneIndex != -1)
                {
                    _levelInstance.Data.Entities[StoneIndex].Room = Convert.ToInt16(StoneSecret.Room);
                    _levelInstance.Data.Entities[StoneIndex].X = StoneSecret.X;
                    _levelInstance.Data.Entities[StoneIndex].Y = StoneSecret.Y;
                    _levelInstance.Data.Entities[StoneIndex].Z = StoneSecret.Z;
                }
                else
                {
                    TR2Entity StoneEntity = new TR2Entity
                    {
                        TypeID = (int)TR2Entities.StoneSecret_S_P,
                        Room = Convert.ToInt16(StoneSecret.Room),
                        X = StoneSecret.X,
                        Y = StoneSecret.Y,
                        Z = StoneSecret.Z,
                        Angle = 0,
                        Intensity1 = -1,
                        Intensity2 = -1,
                        Flags = 0
                    };

                    List<TR2Entity> ents = _levelInstance.Data.Entities.ToList();
                    ents.Add(StoneEntity);
                    _levelInstance.Data.Entities = ents.ToArray();
                    _levelInstance.Data.NumEntities++;
                }
            }
        }

        private void PlaceAllSecrets(List<Location> LevelLocations)
        {
            ZonedLocationCollection ZonedLocations = new ZonedLocationCollection();

            ZonedLocations.PopulateZones(_levelInstance.Name, LevelLocations, ZonePopulationMethod.SecretsOnly);

            List<TR2Entity> ents = _levelInstance.Data.Entities.ToList();

            bool SecretsRemain = true;

            while (SecretsRemain)
            {
                int i;

                //Remove any existing secrets
                for (i = 0; i < ents.Count; i++)
                {
                    if (ents[i].TypeID == (int)TR2Entities.StoneSecret_S_P ||
                        ents[i].TypeID == (int)TR2Entities.JadeSecret_S_P ||
                        ents[i].TypeID == (int)TR2Entities.GoldSecret_S_P)
                    {
                        ents.RemoveAt(i);
                        i--;
                        break;
                    }
                };

                //We have exhausted the list and found nothing, if we exited early try again
                if (i == ents.Count)
                {
                    SecretsRemain = false;
                }
            }

            //Add new entities
            foreach (Location loc in ZonedLocations.StoneZone)
            {
                Location copy = SpatialConverters.TransformToLevelSpace(loc, _levelInstance.Data.Rooms[loc.Room].Info);

                ents.Add(new TR2Entity
                {
                    TypeID = (int)TR2Entities.StoneSecret_S_P,
                    Room = Convert.ToInt16(copy.Room),
                    X = copy.X,
                    Y = copy.Y,
                    Z = copy.Z,
                    Angle = 0,
                    Intensity1 = -1,
                    Intensity2 = -1,
                    Flags = 0
                });
            }

            foreach (Location loc in ZonedLocations.JadeZone)
            {
                Location copy = SpatialConverters.TransformToLevelSpace(loc, _levelInstance.Data.Rooms[loc.Room].Info);

                ents.Add(new TR2Entity
                {
                    TypeID = (int)TR2Entities.JadeSecret_S_P,
                    Room = Convert.ToInt16(copy.Room),
                    X = copy.X,
                    Y = copy.Y,
                    Z = copy.Z,
                    Angle = 0,
                    Intensity1 = -1,
                    Intensity2 = -1,
                    Flags = 0
                });
            }

            foreach (Location loc in ZonedLocations.GoldZone)
            {
                Location copy = SpatialConverters.TransformToLevelSpace(loc, _levelInstance.Data.Rooms[loc.Room].Info);

                ents.Add(new TR2Entity
                {
                    TypeID = (int)TR2Entities.GoldSecret_S_P,
                    Room = Convert.ToInt16(copy.Room),
                    X = copy.X,
                    Y = copy.Y,
                    Z = copy.Z,
                    Angle = 0,
                    Intensity1 = -1,
                    Intensity2 = -1,
                    Flags = 0
                });
            }

            _levelInstance.Data.NumEntities = (uint)ents.Count;
            _levelInstance.Data.Entities = ents.ToArray();
        }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            Dictionary<string, List<Location>> Locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\locations.json"));

            foreach (TR23ScriptedLevel lvl in Levels)
            {
                //Read the level into a level object
                LoadLevelInstance(lvl);

                //Apply the modifications
                RandomizeSecrets(Locations[_levelInstance.Name]);

                //Write back the level file
                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }
    }
}