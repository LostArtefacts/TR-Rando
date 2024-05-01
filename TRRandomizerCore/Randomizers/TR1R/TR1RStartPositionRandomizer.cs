using Newtonsoft.Json;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Utilities;

namespace TRRandomizerCore.Randomizers;

public class TR1RStartPositionRandomizer : BaseTR1RRandomizer
{
    private Dictionary<string, List<Location>> _startLocations;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);
        _startLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR1\Locations\start_positions.json"));

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

    private void RandomizeStartPosition(TR1RCombinedLevel level)
    {
        TR1Entity lara = level.Data.Entities.Find(e => e.TypeID == TR1Type.Lara);

        FDControl floorData = new();
        floorData.ParseFromLevel(level.Data);

        if (!Settings.RotateStartPositionOnly && _startLocations.ContainsKey(level.Name))
        {
            List<Location> locations = _startLocations[level.Name];
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
        }

        short currentAngle = lara.Angle;
        do
        {
            lara.Angle = (short)(_generator.Next(0, 8) * -TRConsts.Angle45);
        }
        while (lara.Angle == currentAngle);
    }
}
