using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TR2RandomizerCore.Helpers;
using TR2RandomizerCore.Utilities;
using TR2RandomizerCore.Zones;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TR2RandomizerCore.Randomizers
{
    public class ItemRandomizer : RandomizerBase
    {
        public bool IncludeKeyItems { get; set; }

        // This replaces plane cargo index as TRGE may have randomized the weaponless level(s), but will also have injected pistols
        // into predefined locations. See FindAndCleanUnarmedPistolLocation below.
        private int _unarmedLevelPistolIndex;
        private readonly Dictionary<string, Location> _pistolLocations;

        public ItemRandomizer()
        {
            _pistolLocations = JsonConvert.DeserializeObject<Dictionary<string, Location>>(File.ReadAllText(@"Resources\unarmed_locations.json"));
        }

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            Dictionary<string, List<Location>> locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText(@"Resources\item_locations.json"));

            foreach (TR23ScriptedLevel lvl in Levels)
            {
                if (SaveMonitor.IsCancelled) return;
                //SaveMonitor.FireSaveStateBeginning(TRSaveCategory.Custom, string.Format("Randomizing items in {0}", lvl.Name));

                //Read the level into a level object
                LoadLevelInstance(lvl);

                FindAndCleanUnarmedPistolLocation(lvl);
                
                if (lvl.Is(LevelNames.HOME))
                {
                    InjectHSHWeaponTextures();
                    CleanHSHCloset();
                }

                //Apply the modifications
                RepositionItems(locations[lvl.LevelFileBaseName.ToUpper()], lvl.LevelFileBaseName.ToUpper());

                //#44 - Randomize OR pistol type
                if (lvl.RemovesWeapons) { RandomizeORPistol(); }
                
                //#47 - Randomize the HSH weapon closet
                if (lvl.Is(LevelNames.HOME)) { PopulateHSHCloset(); }

                //Write back the level file
                SaveLevelInstance();

                SaveMonitor.FireSaveStateChanged(1);
            }
        }

        private void RepositionItems(List<Location> ItemLocs, string lvl)
        {
            if (ItemLocs.Count > 0)
            {
                //We are currently looking guns + ammo
                List<TR2Entities> targetents = TR2EntityUtilities.GetListOfGunTypes();
                targetents.AddRange(TR2EntityUtilities.GetListOfAmmoTypes());

                //And also key items...
                if (IncludeKeyItems)
                {
                    targetents.AddRange(TR2EntityUtilities.GetListOfKeyItemTypes());
                }

                //It's important to now start zoning key items as softlocks must be avoided.
                ZonedLocationCollection ZonedLocations = new ZonedLocationCollection();
                ZonedLocations.PopulateZones(lvl, ItemLocs, ZonePopulationMethod.KeyPuzzleQuestOnly);

                for (int i = 0; i < _levelInstance.Entities.Count(); i++)
                {
                    if (targetents.Contains((TR2Entities)_levelInstance.Entities[i].TypeID) && (i != _unarmedLevelPistolIndex))
                    {
                        Location RandomLocation = new Location();
                        bool FoundPossibleLocation = false;

                        if (TR2EntityUtilities.IsKeyItemType((TR2Entities)_levelInstance.Entities[i].TypeID))
                        {
                            TR2Entities type = (TR2Entities)_levelInstance.Entities[i].TypeID;

                            // Apply zoning for key items
                            switch (type)
                            {
                                case TR2Entities.Puzzle1_S_P:
                                    if (ZonedLocations.Puzzle1Zone.Count > 0)
                                    {
                                        RandomLocation = ZonedLocations.Puzzle1Zone[_generator.Next(0, ZonedLocations.Puzzle1Zone.Count)];
                                        FoundPossibleLocation = true;
                                    }
                                    break;
                                case TR2Entities.Puzzle2_S_P:
                                    if (ZonedLocations.Puzzle2Zone.Count > 0)
                                    {
                                        RandomLocation = ZonedLocations.Puzzle2Zone[_generator.Next(0, ZonedLocations.Puzzle2Zone.Count)];
                                        FoundPossibleLocation = true;
                                    }
                                    break;
                                case TR2Entities.Puzzle3_S_P:
                                    if (ZonedLocations.Puzzle3Zone.Count > 0)
                                    {
                                        RandomLocation = ZonedLocations.Puzzle3Zone[_generator.Next(0, ZonedLocations.Puzzle3Zone.Count)];
                                        FoundPossibleLocation = true;
                                    }
                                    break;
                                case TR2Entities.Puzzle4_S_P:
                                    if (ZonedLocations.Puzzle4Zone.Count > 0)
                                    {
                                        RandomLocation = ZonedLocations.Puzzle4Zone[_generator.Next(0, ZonedLocations.Puzzle4Zone.Count)];
                                        FoundPossibleLocation = true;
                                    }
                                    break;
                                case TR2Entities.Key1_S_P:
                                    if (ZonedLocations.Key1Zone.Count > 0)
                                    {
                                        RandomLocation = ZonedLocations.Key1Zone[_generator.Next(0, ZonedLocations.Key1Zone.Count)];
                                        FoundPossibleLocation = true;
                                    }
                                    break;
                                case TR2Entities.Key2_S_P:
                                    if (ZonedLocations.Key2Zone.Count > 0)
                                    {
                                        RandomLocation = ZonedLocations.Key2Zone[_generator.Next(0, ZonedLocations.Key2Zone.Count)];
                                        FoundPossibleLocation = true;
                                    }
                                    break;
                                case TR2Entities.Key3_S_P:
                                    if (ZonedLocations.Key3Zone.Count > 0)
                                    {
                                        RandomLocation = ZonedLocations.Key3Zone[_generator.Next(0, ZonedLocations.Key3Zone.Count)];
                                        FoundPossibleLocation = true;
                                    }
                                    break;
                                case TR2Entities.Key4_S_P:
                                    if (ZonedLocations.Key4Zone.Count > 0)
                                    {
                                        RandomLocation = ZonedLocations.Key4Zone[_generator.Next(0, ZonedLocations.Key4Zone.Count)];
                                        FoundPossibleLocation = true;
                                    }
                                    break;
                                case TR2Entities.Quest1_S_P:
                                    if (ZonedLocations.Quest1Zone.Count > 0)
                                    {
                                        RandomLocation = ZonedLocations.Quest1Zone[_generator.Next(0, ZonedLocations.Quest1Zone.Count)];
                                        FoundPossibleLocation = true;
                                    }
                                    break;
                                case TR2Entities.Quest2_S_P:
                                    if (ZonedLocations.Quest2Zone.Count > 0)
                                    {
                                        RandomLocation = ZonedLocations.Quest2Zone[_generator.Next(0, ZonedLocations.Quest2Zone.Count)];
                                        FoundPossibleLocation = true;
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            //Place standard items as normal for now
                            RandomLocation = ItemLocs[_generator.Next(0, ItemLocs.Count)];
                            FoundPossibleLocation = true;
                        }

                        if (FoundPossibleLocation)
                        {
                            Location GlobalizedRandomLocation = SpatialConverters.TransformToLevelSpace(RandomLocation, _levelInstance.Rooms[RandomLocation.Room].Info);

                            _levelInstance.Entities[i].Room = Convert.ToInt16(GlobalizedRandomLocation.Room);
                            _levelInstance.Entities[i].X = GlobalizedRandomLocation.X;
                            _levelInstance.Entities[i].Y = GlobalizedRandomLocation.Y;
                            _levelInstance.Entities[i].Z = GlobalizedRandomLocation.Z;
                            _levelInstance.Entities[i].Intensity1 = -1;
                            _levelInstance.Entities[i].Intensity2 = -1;
                        }
                    }
                }
            }
        }

        private void FindAndCleanUnarmedPistolLocation(TR23ScriptedLevel lvl)
        {
            string lvlFile = lvl.LevelFileBaseName.ToUpper();
            Location levelPistolLocation = null;
            if (_pistolLocations.ContainsKey(lvlFile))
            {
                levelPistolLocation = _pistolLocations[lvlFile];
                CleanUnarmedPistolLocation(levelPistolLocation);
            }

            if (lvl.RemovesWeapons && levelPistolLocation != null)
            {
                _unarmedLevelPistolIndex = FindUnarmedPistolIndex(levelPistolLocation);
            }
            else
            {
                _unarmedLevelPistolIndex = -1;
            }
        }

        private int FindUnarmedPistolIndex(Location levelPistolLocation)
        {
            //#44 - Agreed to keep it there but randomize its type.
            return Array.FindIndex(_levelInstance.Entities, e =>
            (
                e.TypeID == (int)TR2Entities.Pistols_S_P ||
                e.TypeID == (int)TR2Entities.Shotgun_S_P ||
                e.TypeID == (int)TR2Entities.Automags_S_P ||
                e.TypeID == (int)TR2Entities.Uzi_S_P ||
                e.TypeID == (int)TR2Entities.Harpoon_S_P ||
                e.TypeID == (int)TR2Entities.M16_S_P ||
                e.TypeID == (int)TR2Entities.GrenadeLauncher_S_P
            ) &&
            (
                e.Room == levelPistolLocation.Room &&
                e.X == levelPistolLocation.X &&
                e.Y == levelPistolLocation.Y &&
                e.Z == levelPistolLocation.Z
            ));
        }

        private void CleanUnarmedPistolLocation(Location levelPistolLocation)
        {
            //In relation to #66 to ensure successive randomizations don't pollute the entity list
            List<TR2Entity> entities = _levelInstance.Entities.ToList();

            // We need to ensure x,y,z also match rather than just the room as TRGE could have added pistols
            // (which may then have been randomized and/or ammo added) to a room that already had other ammo pickups.
            IEnumerable<TR2Entity> existingInjections = entities.Where
            (
                e =>
                    TR2EntityUtilities.IsAmmoType((TR2Entities)e.TypeID) &&
                    e.Room == levelPistolLocation.Room &&
                    e.X == levelPistolLocation.X &&
                    e.Y == levelPistolLocation.Y &&
                    e.Z == levelPistolLocation.Z
            );

            if (existingInjections.Count() > 0)
            {
                // For Rig, if it's no longer unarmed, TRGE will have added UZI clips where the pistols normally
                // would be. This is to preserve item indices as the pistols have index 4 - to remove them completely
                // would mean anything that points to higher item indices (triggers etc) would need to change. The clips
                // can be safely randomized - it's just the index that needs to remain the same.
                if (_scriptedLevelInstance.Is(LevelNames.RIG))
                {
                    TR2Entity cargoEntity = existingInjections.FirstOrDefault();
                    entities.RemoveAll(e => existingInjections.Contains(e) && e != cargoEntity);
                }
                else
                {
                    entities.RemoveAll(e => existingInjections.Contains(e));
                }
                _levelInstance.NumEntities = (uint)entities.Count;
                _levelInstance.Entities = entities.ToArray();
            }            
        }

        private readonly Dictionary<TR2Entities, uint> _startingAmmoToGive = new Dictionary<TR2Entities, uint>()
        {
            {TR2Entities.Shotgun_S_P, 8},
            {TR2Entities.Automags_S_P, 4},
            {TR2Entities.Uzi_S_P, 4},
            {TR2Entities.Harpoon_S_P, 24},
            {TR2Entities.M16_S_P, 2},
            {TR2Entities.GrenadeLauncher_S_P, 4},
        };

        private void RandomizeORPistol()
        {
            //Is there something in the unarmed level pistol location?
            if (_unarmedLevelPistolIndex != -1)
            {
                List<TR2Entities> ReplacementWeapons = TR2EntityUtilities.GetListOfGunTypes();
                ReplacementWeapons.Add(TR2Entities.Pistols_S_P);

                TR2Entities Weap = ReplacementWeapons[_generator.Next(0, ReplacementWeapons.Count)];
                if (_scriptedLevelInstance.Is(LevelNames.CHICKEN))
                {
                    // Grenade Launcher and Harpoon cannot trigger the bells in Ice Palace
                    while (Weap.Equals(TR2Entities.GrenadeLauncher_S_P) || Weap.Equals(TR2Entities.Harpoon_S_P))
                    {
                        Weap = ReplacementWeapons[_generator.Next(0, ReplacementWeapons.Count)];
                    }
                }

                TR2Entity unarmedLevelWeapons = _levelInstance.Entities[_unarmedLevelPistolIndex];

                uint ammoToGive = 0;
                if (_startingAmmoToGive.ContainsKey(Weap))
                {
                    ammoToGive = _startingAmmoToGive[Weap];
                    if (_scriptedLevelInstance.Is(LevelNames.LAIR))
                        ammoToGive *= 6;
                }

                //#68 - Provide some additional ammo for a weapon if not pistols
                switch (Weap)
                {
                    case TR2Entities.Shotgun_S_P:
                        AddORAmmo(TR2Entities.ShotgunAmmo_S_P, ammoToGive, unarmedLevelWeapons);
                        break;
                    case TR2Entities.Automags_S_P:
                        AddORAmmo(TR2Entities.AutoAmmo_S_P, ammoToGive, unarmedLevelWeapons);
                        break;
                    case TR2Entities.Uzi_S_P:
                        AddORAmmo(TR2Entities.UziAmmo_S_P, ammoToGive, unarmedLevelWeapons);
                        break;
                    case TR2Entities.Harpoon_S_P:
                        AddORAmmo(TR2Entities.HarpoonAmmo_S_P, ammoToGive, unarmedLevelWeapons);
                        break;
                    case TR2Entities.M16_S_P:
                        AddORAmmo(TR2Entities.M16Ammo_S_P, ammoToGive, unarmedLevelWeapons);
                        break;
                    case TR2Entities.GrenadeLauncher_S_P:
                        AddORAmmo(TR2Entities.Grenades_S_P, ammoToGive, unarmedLevelWeapons);
                        break;
                    default:
                        break;
                }

                unarmedLevelWeapons.TypeID = (short)Weap;
            }
        }

        private void AddORAmmo(TR2Entities ammoType, uint count, TR2Entity weapon)
        {
            List<TR2Entity> ents = _levelInstance.Entities.ToList();

            for (uint i = 0; i < count; i++)
            {
                TR2Entity ammo = weapon.Clone();

                ammo.TypeID = (short)ammoType;

                ents.Add(ammo);
            };

            _levelInstance.NumEntities += count;
            _levelInstance.Entities = ents.ToArray();
        }

        private void InjectHSHWeaponTextures()
        {
            // All textures, animations and moveables are available as standard in HSH via TRGE
        }

        private void CleanHSHCloset()
        {

        }

        private void PopulateHSHCloset()
        {

        }
    }
}