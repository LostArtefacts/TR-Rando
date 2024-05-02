using Newtonsoft.Json;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl;

namespace TRRandomizerCore.Randomizers;

public class TR2RStartPositionRandomizer : BaseTR2RRandomizer
{
    private Dictionary<string, List<Location>> _startLocations;

    public override void Randomize(int seed)
    {
        _generator = new(seed);
        _startLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR2\Locations\start_positions.json"));

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

    private void RandomizeStartPosition(TR2RCombinedLevel level)
    {
        if (level.Script.HasStartAnimation)
        {
            return;
        }

        TR2Entity lara = level.Data.Entities.Find(e => e.TypeID == TR2Type.Lara);

        if (level.Data.Entities.Find(e => e.Room == lara.Room
            && TR2TypeUtilities.IsSecretType(e.TypeID)) != null)
        {
            return;
        }

        if (!Settings.RotateStartPositionOnly && _startLocations.ContainsKey(level.Name))
        {
            List<Location> locations = _startLocations[level.Name];
            Location location = locations[_generator.Next(0, locations.Count)];
            lara.Room = location.Room;
            lara.X = location.X;
            lara.Y = location.Y;
            lara.Z = location.Z;
            lara.Angle = location.Angle;
        }

        short currentAngle = lara.Angle;
        do
        {
            lara.Angle = (short)(_generator.Next(0, 8) * -TRConsts.Angle45);
        }
        while (lara.Angle == currentAngle);

        if (level.Is(TR2LevelNames.BARTOLI))
        {
            TR2Entity boat = level.Data.Entities.Find(e => e.TypeID == TR2Type.Boat);
            boat.Angle = lara.Angle;
        }
    }
}
