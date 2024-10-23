using Newtonsoft.Json;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRLevelControl;

namespace TRRandomizerCore.Randomizers;

public class TR2StartPositionRandomizer : BaseTR2Randomizer
{
    private Dictionary<string, List<Location>> _startLocations;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);
        _startLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource("TR2/Locations/start_positions.json"));

        foreach (TR2ScriptedLevel lvl in Levels)
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

    private void RandomizeStartPosition(TR2CombinedLevel level)
    {
        if (level.Script.HasStartAnimation)
        {
            // Don't change either the position or angle in Rig or HSH as the start cutscene looks odd and
            // for HSH Lara doesn't end up on the trigger for the enemies.
            return;
        }

        TR2Entity lara = level.Data.Entities.Find(e => e.TypeID == TR2Type.Lara);

        // We only change position if there is not a secret in the same room as Lara, This is just in case it ends up
        // where she starts on a slope (GW or Opera House for example), as its X,Y,Z values may not be identical to Lara's,
        // or she may have to jump on the first frame to get it.
        if (!Settings.DevelopmentMode
            && level.Data.Entities.Find(e => e.Room == lara.Room
            && TR2TypeUtilities.IsSecretType(e.TypeID)) != null)
        {
            return;
        }

        // If we haven't defined anything for a level, Lara will just be rotated. This is most likely where there are
        // triggers just after Lara's starting spot, so we just skip them here.
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

        // Spin the boat around too
        if (level.Is(TR2LevelNames.BARTOLI))
        {
            TR2Entity boat = level.Data.Entities.Find(e => e.TypeID == TR2Type.Boat);
            boat.Angle = lara.Angle;
        }
    }
}
