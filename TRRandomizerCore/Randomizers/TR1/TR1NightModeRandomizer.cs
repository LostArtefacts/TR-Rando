using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TR1NightModeRandomizer : BaseTR1Randomizer
{
    public const uint DarknessRange = 10; // 0 = Dusk, 10 = Night

    private static readonly Dictionary<string, List<int>> _excludedRooms = new()
    {
        [TR1LevelNames.ATLANTIS]
            = new List<int> { 85, 95, 96 } // We want to retain the flicker effect at the start
    };

    internal TR1TextureMonitorBroker TextureMonitor { get; set; }

    private List<TR1ScriptedLevel> _nightLevels;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        Settings.NightModeDarkness = Math.Min(Settings.NightModeDarkness, DarknessRange);

        ChooseNightLevels();

        foreach (TR1ScriptedLevel lvl in Levels)
        {
            LoadLevelInstance(lvl);

            if (_nightLevels.Contains(lvl))
            {
                SetNightMode(_levelInstance);
                SaveLevelInstance();
            }

            if (!TriggerProgress())
            {
                break;
            }
        }
    }

    private void ChooseNightLevels()
    {
        TR1ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR1LevelNames.ASSAULT));
        ISet<TR1ScriptedLevel> exlusions = new HashSet<TR1ScriptedLevel> { assaultCourse };

        _nightLevels = Levels.RandomSelection(_generator, (int)Settings.NightModeCount, exclusions: exlusions);
        if (Settings.NightModeAssaultCourse)
        {
            _nightLevels.Add(assaultCourse);
        }
    }

    private void SetNightMode(TR1CombinedLevel level)
    {
        DarkenRooms(level);

        if (level.HasCutScene)
        {
            SetNightMode(level.CutSceneLevel);
        }

        // Notify the texture monitor that this level is now in night mode
        TextureMonitor<TR1Type> monitor = TextureMonitor.CreateMonitor(level.Name);
        monitor.UseNightTextures = true;
    }

    private void DarkenRooms(TR1CombinedLevel level)
    {
        double scale = (100 - DarknessRange + Settings.NightModeDarkness) / 100d;

        short intensity1 = (short)(TR2Room.DarknessIntensity1 * scale);
        ushort intensity2 = (ushort)(TR2Room.DarknessIntensity2 * (2 - scale));

        for (int i = 0; i < level.Data.Rooms.Count; i++)
        {
            if (_excludedRooms.ContainsKey(level.Name) && _excludedRooms[level.Name].Contains(i))
            {
                continue;
            }

            TR1Room room = level.Data.Rooms[i];

            room.AmbientIntensity = intensity1;
            room.Lights.ForEach(l => l.Intensity = intensity2);
            room.StaticMeshes.ForEach(s => s.Intensity = (ushort)intensity1);
            room.RoomData.Vertices.ToList().ForEach(v => v.Lighting = intensity1);
        }
    }
}
