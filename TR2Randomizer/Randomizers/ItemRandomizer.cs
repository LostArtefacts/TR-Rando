using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TR2Randomizer.Utilities;
using TRLevelReader.Helpers;
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

            Dictionary<string, List<Location>> Locations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(File.ReadAllText("locations.json"));

            foreach (string lvl in _levels)
            {
                //Read the level into a level object
                _levelInstance = _reader.ReadLevel(lvl);

                //#44 - Randomize OR pistol type
                if (lvl == LevelNames.RIG)
                {
                    RandomizeORPistol();
                }

                //Apply the modifications
                RepositionItems(Locations[lvl]);

                //Write back the level file
                _writer.WriteLevelToFile(_levelInstance, lvl);

                ReplacementStatusManager.LevelProgress++;
            }

            ReplacementStatusManager.LevelProgress = 0;
            ReplacementStatusManager.CanRandomize = true;
        }

        private void RepositionItems(List<Location> ItemLocs)
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

        private void RandomizeORPistol()
        {
            //#44 - Agreed to keep it there but randomize its type.
            _planeCargoWeaponIndex = Array.FindIndex(_levelInstance.Entities, 
                e => (  e.TypeID == (int)TR2Entities.Pistols_S_P || 
                        e.TypeID == (int)TR2Entities.Shotgun_S_P ||
                        e.TypeID == (int)TR2Entities.Automags_S_P ||
                        e.TypeID == (int)TR2Entities.Uzi_S_P ||
                        e.TypeID == (int)TR2Entities.Harpoon_S_P ||
                        e.TypeID == (int)TR2Entities.M16_S_P ||
                        e.TypeID == (int)TR2Entities.GrenadeLauncher_S_P) && (e.Room == 1));

            //Is there something in the plane cargo?
            if (_planeCargoWeaponIndex != -1)
            {
                List<TR2Entities> ReplacementWeapons = TR2EntityUtilities.GetListOfGunTypes();
                ReplacementWeapons.Add(TR2Entities.Pistols_S_P);

                _levelInstance.Entities[_planeCargoWeaponIndex].TypeID = (short)ReplacementWeapons[_generator.Next(0, ReplacementWeapons.Count)];
            }
        }
    }
}
