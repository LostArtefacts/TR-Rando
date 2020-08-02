using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TR2Randomizer.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TR2Randomizer.Randomizers
{
    public class ItemRandomizer : RandomizerBase
    {
        private int _planeCargoWeaponIndex;
        public ItemRandomizer() : base()
        {
        }

        public override void Randomize(int seed)
        {
            ReplacementStatusManager.CanRandomize = false;

            _generator = new Random(seed);

            Dictionary<string, List<Location>> Locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText("item_locations.json"));

            foreach (string lvl in _levels)
            {
                //Read the level into a level object
                _levelInstance = LoadLevel(lvl);

                if (lvl == LevelNames.RIG) { CleanPlaneCargo(); FindPlaneCargoIndex(); }
                if (lvl == LevelNames.HOME) { InjectHSHWeaponTextures(); CleanHSHCloset(); }

                //Apply the modifications
                RepositionItems(Locations[lvl]);

                //#44 - Randomize OR pistol type
                if (lvl == LevelNames.RIG) { RandomizeORPistol(); }
                
                //#47 - Randomize the HSH weapon closet
                if (lvl == LevelNames.HOME) { PopulateHSHCloset(); }

                //Write back the level file
                SaveLevel(_levelInstance, lvl);

                ReplacementStatusManager.LevelProgress++;
            }

            ReplacementStatusManager.LevelProgress = 0;
            ReplacementStatusManager.CanRandomize = true;
        }

        private void RepositionItems(List<Location> ItemLocs)
        {
            if (ItemLocs.Count > 0)
            {
                //We are currently looking guns + ammo
                List<TR2Entities> targetents = TR2EntityUtilities.GetListOfGunTypes();
                targetents.AddRange(TR2EntityUtilities.GetListOfAmmoTypes());

                for (int i = 0; i < _levelInstance.Entities.Count(); i++)
                {
                    if (targetents.Contains((TR2Entities)_levelInstance.Entities[i].TypeID) && (i != _planeCargoWeaponIndex))
                    {
                        Location RandomLocation = ItemLocs[_generator.Next(0, ItemLocs.Count)];

                        Location GlobalizedRandomLocation = SpatialConverters.TransformToLevelSpace(RandomLocation, _levelInstance.Rooms[RandomLocation.Room].Info);

                        _levelInstance.Entities[i].Room = Convert.ToInt16(GlobalizedRandomLocation.Room);
                        _levelInstance.Entities[i].X = GlobalizedRandomLocation.X;
                        _levelInstance.Entities[i].Y = GlobalizedRandomLocation.Y;
                        _levelInstance.Entities[i].Z = GlobalizedRandomLocation.Z;
                    }
                }
            }
        }

        private void FindPlaneCargoIndex()
        {
            //#44 - Agreed to keep it there but randomize its type.
            _planeCargoWeaponIndex = Array.FindIndex(_levelInstance.Entities,
                e => (e.TypeID == (int)TR2Entities.Pistols_S_P ||
                        e.TypeID == (int)TR2Entities.Shotgun_S_P ||
                        e.TypeID == (int)TR2Entities.Automags_S_P ||
                        e.TypeID == (int)TR2Entities.Uzi_S_P ||
                        e.TypeID == (int)TR2Entities.Harpoon_S_P ||
                        e.TypeID == (int)TR2Entities.M16_S_P ||
                        e.TypeID == (int)TR2Entities.GrenadeLauncher_S_P) && (e.Room == 1));
        }

        private void CleanPlaneCargo()
        {
            //In relation to #66 to ensure successive randomizations don't pollute the entity list
            List<TR2Entity> Entities = _levelInstance.Entities.ToList();

            int index = Entities.FindIndex(e => (e.Room == 1 && TR2EntityUtilities.IsAmmoType((TR2Entities)e.TypeID)));

            while (index != -1 && index < Entities.Count)
            {
                Entities.RemoveAt(index);
                _levelInstance.NumEntities--;
                index = Entities.FindIndex(e => (e.Room == 1 && TR2EntityUtilities.IsAmmoType((TR2Entities)e.TypeID)));
            }

            _levelInstance.Entities = Entities.ToArray();
        }

        private void RandomizeORPistol()
        {
            //Is there something in the plane cargo?
            if (_planeCargoWeaponIndex != -1)
            {
                List<TR2Entities> ReplacementWeapons = TR2EntityUtilities.GetListOfGunTypes();
                ReplacementWeapons.Add(TR2Entities.Pistols_S_P);

                TR2Entities Weap = ReplacementWeapons[_generator.Next(0, ReplacementWeapons.Count)];

                TR2Entity CargoWeapon =  _levelInstance.Entities[_planeCargoWeaponIndex];

                //#68 - Provide some additional ammo for a weapon if not pistols
                switch (Weap)
                {
                    case TR2Entities.Shotgun_S_P:
                        AddORAmmo(TR2Entities.ShotgunAmmo_S_P, 8, CargoWeapon);
                        break;
                    case TR2Entities.Automags_S_P:
                        AddORAmmo(TR2Entities.AutoAmmo_S_P, 4, CargoWeapon);
                        break;
                    case TR2Entities.Uzi_S_P:
                        AddORAmmo(TR2Entities.UziAmmo_S_P, 4, CargoWeapon);
                        break;
                    case TR2Entities.Harpoon_S_P:
                        AddORAmmo(TR2Entities.HarpoonAmmo_S_P, 10, CargoWeapon);
                        break;
                    case TR2Entities.M16_S_P:
                        AddORAmmo(TR2Entities.M16Ammo_S_P, 2, CargoWeapon);
                        break;
                    case TR2Entities.GrenadeLauncher_S_P:
                        AddORAmmo(TR2Entities.GrenadeLauncher_S_P, 4, CargoWeapon);
                        break;
                    default:
                        break;
                }

                CargoWeapon.TypeID = (short)Weap;
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

        }

        private void CleanHSHCloset()
        {

        }

        private void PopulateHSHCloset()
        {

        }
    }
}
