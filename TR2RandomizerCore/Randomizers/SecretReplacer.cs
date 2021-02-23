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
        //public bool PlaceAll { get; set; }
        private bool PlaceAll; //removed from public interface
        public bool AllowHard { get; set; }
        
        public SecretReplacer() : base()
        {
            PlaceAll = false;
        }

        private void RandomizeSecrets(List<Location> LevelLocations, TR23ScriptedLevel lvl)
        {
            if (LevelLocations.Count > 2)
            {
                if (false && PlaceAll)
                {
                    PlaceAllSecrets(lvl, LevelLocations);
                    return;
                }

                //Apply zoning to the locations to ensure they are spread out.
                ZonedLocationCollection ZonedLocations = new ZonedLocationCollection();

                ZonedLocations.PopulateZones(lvl.LevelFileBaseName.ToUpper(), LevelLocations, ZonePopulationMethod.SecretsOnly);

                Location GoldSecret;
                Location JadeSecret;
                Location StoneSecret;

                //Find suitable locations, ensuring they are zoned, do not share a room and difficulty.
                //Location = ZoneLocations[ZoneGroup][LocationInZoneGroup]
                do
                {
                    GoldSecret = ZonedLocations.GoldZone[_generator.Next(0, ZonedLocations.GoldZone.Count)];
                } while (GoldSecret.Difficulty == Difficulty.Hard && AllowHard == false);
                

                do
                {
                    JadeSecret = ZonedLocations.JadeZone[_generator.Next(0, ZonedLocations.JadeZone.Count)];
                } while ((JadeSecret.Room == GoldSecret.Room) || 
                        (JadeSecret.Difficulty == Difficulty.Hard && AllowHard == false));

                do
                {
                    StoneSecret = ZonedLocations.StoneZone[_generator.Next(0, ZonedLocations.StoneZone.Count)];
                } while ((StoneSecret.Room == GoldSecret.Room) || 
                        (StoneSecret.Room == JadeSecret.Room) ||
                        (StoneSecret.Difficulty == Difficulty.Hard && AllowHard == false));

                //Due to TRMod only accepting room space coords entities are actually stored in level space. So include some
                //calls to support a transformation of any locations that are specified in room space to maintain backwards compatbility
                //with older locations and support locations that are specified in both level or room space.
                GoldSecret = SpatialConverters.TransformToLevelSpace(GoldSecret, _levelInstance.Rooms[GoldSecret.Room].Info);
                JadeSecret = SpatialConverters.TransformToLevelSpace(JadeSecret, _levelInstance.Rooms[JadeSecret.Room].Info);
                StoneSecret = SpatialConverters.TransformToLevelSpace(StoneSecret, _levelInstance.Rooms[StoneSecret.Room].Info);

                //Does the level contain the entities?
                int GoldIndex = Array.FindIndex(_levelInstance.Entities, ent => (ent.TypeID == (short)TR2Entities.GoldSecret_S_P));
                int JadeIndex = Array.FindIndex(_levelInstance.Entities, ent => (ent.TypeID == (short)TR2Entities.JadeSecret_S_P));
                int StoneIndex = Array.FindIndex(_levelInstance.Entities, ent => (ent.TypeID == (short)TR2Entities.StoneSecret_S_P));

                //Check if we could find instances of the secret entities, if not, we need to add not edit.
                if (GoldIndex != -1)
                {
                    _levelInstance.Entities[GoldIndex].Room = Convert.ToInt16(GoldSecret.Room);
                    _levelInstance.Entities[GoldIndex].X = GoldSecret.X;
                    _levelInstance.Entities[GoldIndex].Y = GoldSecret.Y;
                    _levelInstance.Entities[GoldIndex].Z = GoldSecret.Z;
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

                    List<TR2Entity> ents = _levelInstance.Entities.ToList();
                    ents.Add(GoldEntity);
                    _levelInstance.Entities = ents.ToArray();
                    _levelInstance.NumEntities++;
                }

                if (JadeIndex != -1)
                {
                    _levelInstance.Entities[JadeIndex].Room = Convert.ToInt16(JadeSecret.Room);
                    _levelInstance.Entities[JadeIndex].X = JadeSecret.X;
                    _levelInstance.Entities[JadeIndex].Y = JadeSecret.Y;
                    _levelInstance.Entities[JadeIndex].Z = JadeSecret.Z;
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

                    List<TR2Entity> ents = _levelInstance.Entities.ToList();
                    ents.Add(JadeEntity);
                    _levelInstance.Entities = ents.ToArray();
                    _levelInstance.NumEntities++;
                }

                if (StoneIndex != -1)
                {
                    _levelInstance.Entities[StoneIndex].Room = Convert.ToInt16(StoneSecret.Room);
                    _levelInstance.Entities[StoneIndex].X = StoneSecret.X;
                    _levelInstance.Entities[StoneIndex].Y = StoneSecret.Y;
                    _levelInstance.Entities[StoneIndex].Z = StoneSecret.Z;
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

                    List<TR2Entity> ents = _levelInstance.Entities.ToList();
                    ents.Add(StoneEntity);
                    _levelInstance.Entities = ents.ToArray();
                    _levelInstance.NumEntities++;
                }
            }
        }

        private void PlaceAllSecrets(TR23ScriptedLevel lvl, List<Location> LevelLocations)
        {
            ZonedLocationCollection ZonedLocations = new ZonedLocationCollection();

            ZonedLocations.PopulateZones(lvl.LevelFileBaseName.ToUpper(), LevelLocations, ZonePopulationMethod.SecretsOnly);

            List<TR2Entity> ents = _levelInstance.Entities.ToList();

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
                Location copy = SpatialConverters.TransformToLevelSpace(loc, _levelInstance.Rooms[loc.Room].Info);

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
                Location copy = SpatialConverters.TransformToLevelSpace(loc, _levelInstance.Rooms[loc.Room].Info);

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
                Location copy = SpatialConverters.TransformToLevelSpace(loc, _levelInstance.Rooms[loc.Room].Info);

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

            _levelInstance.NumEntities = (uint)ents.Count;
            _levelInstance.Entities = ents.ToArray();
        }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            Dictionary<string, List<Location>> Locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\locations.json"));

            foreach (TR23ScriptedLevel lvl in Levels)
            {
                if (SaveMonitor.IsCancelled) return;
                //SaveMonitor.FireSaveStateBeginning(TRSaveCategory.Custom, string.Format("Randomizing secrets in {0}", lvl.Name));

                //Read the level into a level object
                LoadLevelInstance(lvl);

                //Apply the modifications
                RandomizeSecrets(Locations[lvl.LevelFileBaseName.ToUpper()], lvl);

                //Write back the level file
                SaveLevelInstance();

                SaveMonitor.FireSaveStateChanged(1);
            }
        }
    }
}