using Newtonsoft.Json;
using TRDataControl.Environment;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;

namespace TRRandomizerCore.Randomizers;

public class TR3RStartPositionRandomizer : BaseTR3RRandomizer
{
    private Dictionary<string, List<Location>> _startLocations;

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _startLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR3\Locations\start_positions.json"));

        foreach (TRRScriptedLevel lvl in Levels)
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

    private void RandomizeStartPosition(TR3RCombinedLevel level)
    {
        TR3Entity lara = level.Data.Entities.Find(e => e.TypeID == TR3Type.Lara);

        if (!Settings.RotateStartPositionOnly && _startLocations.ContainsKey(level.Name))
        {
            List<Location> locations = _startLocations[level.Name];
            Location location;
            do
            {
                location = locations[_generator.Next(0, locations.Count)];
            }
            while (!location.Validated);

            new EMMoveTriggerFunction
            {
                BaseLocation = new()
                {
                    X = lara.X,
                    Y = lara.Y,
                    Z = lara.Z,
                    Room = lara.Room
                },
                NewLocation = new()
                {
                    X = location.X,
                    Y = location.Y,
                    Z = location.Z,
                    Room = location.Room
                }
            }.ApplyToLevel(level.Data);

            lara.X = location.X;
            lara.Y = location.Y;
            lara.Z = location.Z;
            lara.Room = location.Room;
        }

        short currentAngle = lara.Angle;
        do
        {
            lara.Angle = (short)(_generator.Next(0, 8) * -TRConsts.Angle45);
        }
        while (lara.Angle == currentAngle);
    }
}
