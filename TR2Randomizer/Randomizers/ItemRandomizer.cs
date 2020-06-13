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
            //We are currently looking for any ammo or key items
            List<TR2Entities> targetents = TR2EntityUtilities.GetListOfGunAmmoTypes();

            for (int i = 0; i < _levelInstance.Entities.Count(); i++)
            {
                if (targetents.Contains((TR2Entities)_levelInstance.Entities[i].TypeID))
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
}
