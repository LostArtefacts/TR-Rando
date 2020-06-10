using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TR2Randomizer.Utilities;
using TRLevelReader;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TR2Randomizer
{
    public class ItemRandomizer
    {
        private TR2LevelReader _reader;
        private TR2LevelWriter _writer;
        private TR2Level _levelInstance;
        private Random _generator;
        private List<string> _levels;

        public ItemRandomizer()
        {
            _levels = LevelNames.AsList;

            _reader = new TR2LevelReader();
            _writer = new TR2LevelWriter();
        }

        public void Randomize(int seed)
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
            targetents.AddRange(TR2EntityUtilities.GetListOfKeyItemTypes());

            foreach (TR2Entity ent in _levelInstance.Entities)
            {
                if (targetents.Contains((TR2Entities)ent.TypeID))
                {
                    Location RandomLocation = ItemLocs[_generator.Next(0, ItemLocs.Count)];

                    RandomLocation = SpatialConverters.TransformToLevelSpace(RandomLocation, _levelInstance.Rooms[RandomLocation.Room]);

                    ent.X = RandomLocation.X;
                    ent.Y = RandomLocation.Y;
                    ent.Z = RandomLocation.Z;
                }
            }
        }
    }
}
