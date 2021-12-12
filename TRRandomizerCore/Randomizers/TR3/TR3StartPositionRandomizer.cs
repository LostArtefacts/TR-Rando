using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TREnvironmentEditor.Model.Types;
using TRGE.Core;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Randomizers
{
    public class TR3StartPositionRandomizer : BaseTR3Randomizer
    {
        private static readonly short _rotation = -8192;
        private Dictionary<string, List<Location>> _startLocations;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);
            _startLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\start_positions.json"));

            foreach (TR3ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                RandomizeStartPosition(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void RandomizeStartPosition(TR3CombinedLevel level)
        {
            List<TR2Entity> entities = level.Data.Entities.ToList();
            TR2Entity lara = entities.Find(e => e.TypeID == (short)TR3Entities.Lara);

            // If we haven't defined anything for a level, Lara will just be rotated. This is most likely where there are
            // triggers just after Lara's starting spot, so we just skip them here.
            if (!Settings.RotateStartPositionOnly && _startLocations.ContainsKey(level.Name))
            {
                List<Location> locations = _startLocations[level.Name];
                if (Settings.DevelopmentMode)
                {
                    foreach (Location loc in locations)
                    {
                        entities.Add(new TR2Entity
                        {
                            TypeID = (short)TR3Entities.Lara,
                            X = loc.X,
                            Y = loc.Y,
                            Z = loc.Z,
                            Room = (short)loc.Room,
                            Angle = lara.Angle,
                            Intensity1 = -1,
                            Intensity2 = -1,
                            Flags = 0
                        });
                    }
                }
                else
                {
                    Location location;
                    do
                    {
                        location = locations[_generator.Next(0, locations.Count)];
                    }
                    while (!location.Validated);

                    // If there are any triggers below Lara, move them
                    new EMMoveTriggerFunction
                    {
                        BaseLocation = new EMLocation
                        {
                            X = lara.X,
                            Y = lara.Y,
                            Z = lara.Z,
                            Room = lara.Room
                        },
                        NewLocation = new EMLocation
                        {
                            X = location.X,
                            Y = location.Y,
                            Z = location.Z,
                            Room = (short)location.Room
                        }
                    }.ApplyToLevel(level.Data);

                    lara.X = location.X;
                    lara.Y = location.Y;
                    lara.Z = location.Z;
                    lara.Room = (short)location.Room;
                    lara.Angle = (short)(_generator.Next(0, 8) * _rotation);
                }
            }
            else
            {
                short currentAngle = lara.Angle;
                do
                {
                    lara.Angle = (short)(_generator.Next(0, 8) * _rotation);
                }
                while (lara.Angle == currentAngle);
            }

            if (Settings.DevelopmentMode)
            {
                level.Data.Entities = entities.ToArray();
                level.Data.NumEntities = (uint)entities.Count;
            }
        }
    }
}