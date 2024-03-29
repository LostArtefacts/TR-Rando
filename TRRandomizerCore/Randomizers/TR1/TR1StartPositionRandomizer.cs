﻿using Newtonsoft.Json;
using TRFDControl;
using TRGE.Core;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1StartPositionRandomizer : BaseTR1Randomizer
{
    private static readonly short _rotation = -8192;
    private Dictionary<string, List<Location>> _startLocations;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);
        _startLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\start_positions.json"));

        foreach (TR1ScriptedLevel lvl in Levels)
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

    private void RandomizeStartPosition(TR1CombinedLevel level)
    {
        TR1Entity lara = level.Data.Entities.Find(e => e.TypeID == (short)TR1Type.Lara);

        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        // If we haven't defined anything for a level, Lara will just be rotated. This is most likely where there are
        // triggers just after Lara's starting spot, so we just skip them here.
        if (!Settings.RotateStartPositionOnly && _startLocations.ContainsKey(level.Name))
        {
            List<Location> locations = _startLocations[level.Name];
            if (Settings.DevelopmentMode)
            {
                foreach (Location loc in locations)
                {
                    level.Data.Entities.Add(new()
                    {
                        TypeID = (short)TR1Type.Lara,
                        X = loc.X,
                        Y = loc.Y,
                        Z = loc.Z,
                        Room = (short)loc.Room,
                        Angle = lara.Angle
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
                while (!location.Validated || location.ContainsSecret(level.Data, floorData));

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
    }
}
