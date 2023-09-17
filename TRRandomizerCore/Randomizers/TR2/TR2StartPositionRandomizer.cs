using Newtonsoft.Json;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TRRandomizerCore.Randomizers;

public class TR2StartPositionRandomizer : BaseTR2Randomizer
{
    private Dictionary<string, List<Location>> _startLocations;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);
        _startLocations = JsonConvert.DeserializeObject<Dictionary<string, List<Location>>>(ReadResource(@"TR2\Locations\start_positions.json"));

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

        List<TR2Entity> entities = level.Data.Entities.ToList();
        TR2Entity lara = entities.Find(e => e.TypeID == (short)TR2Entities.Lara);

        // We only change position if there is not a secret in the same room as Lara, This is just in case it ends up
        // where she starts on a slope (GW or Opera House for example), as its X,Y,Z values may not be identical to Lara's,
        // or she may have to jump on the first frame to get it.
        if (!Settings.DevelopmentMode && entities.Find(e => e.Room == lara.Room && TR2EntityUtilities.IsSecretType((TR2Entities)e.TypeID)) != null)
        {
            return;
        }

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
                        TypeID = (short)TR2Entities.Lara,
                        Room = Convert.ToInt16(loc.Room),
                        X = loc.X,
                        Y = loc.Y,
                        Z = loc.Z,
                        Angle = 0,
                        Intensity1 = -1,
                        Intensity2 = -1,
                        Flags = 0
                    });
                }
            }
            else
            {
                Location location = locations[_generator.Next(0, locations.Count)];
                lara.Room = (short)location.Room;
                lara.X = location.X;
                lara.Y = location.Y;
                lara.Z = location.Z;
                lara.Angle = location.Angle;
            }
        }

        RotateLara(lara, level);

        if (Settings.DevelopmentMode)
        {
            level.Data.Entities = entities.ToArray();
            level.Data.NumEntities = (uint)entities.Count;
        }
    }

    private void RotateLara(TR2Entity lara, TR2CombinedLevel level)
    {
        short currentAngle = lara.Angle;
        do
        {
            int degrees = 45 * _generator.Next(0, 8);
            lara.Angle = (short)(degrees * 16384 / -90);
        }
        while (lara.Angle == currentAngle);

        // Spin the boat around too
        if (level.Is(TR2LevelNames.BARTOLI))
        {
            TR2Entity boat = level.Data.Entities.ToList().Find(e => e.TypeID == (short)TR2Entities.Boat);
            boat.Angle = lara.Angle;
        }
    }
}
