using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TR3NightModeRandomizer : BaseTR3Randomizer
{
    public const uint DarknessRange = 10; // 0 = Dusk, 10 = Night

    private List<TR3ScriptedLevel> _nightLevels;

    internal TR3TextureMonitorBroker TextureMonitor { get; set; }

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        Settings.NightModeDarkness = Math.Min(Settings.NightModeDarkness, DarknessRange);

        ChooseNightLevels();

        foreach (TR3ScriptedLevel lvl in Levels)
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
        TR3ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR3LevelNames.ASSAULT));
        ISet<TR3ScriptedLevel> exlusions = new HashSet<TR3ScriptedLevel> { assaultCourse };

        _nightLevels = Levels.RandomSelection(_generator, (int)Settings.NightModeCount, exclusions: exlusions);
        if (Settings.NightModeAssaultCourse)
        {
            _nightLevels.Add(assaultCourse);
        }
    }

    private void SetNightMode(TR3CombinedLevel level)
    {
        DarkenRooms(level.Data);

        if (level.HasCutScene)
        {
            SetNightMode(level.CutSceneLevel);
        }

        // Notify the texture monitor that this level is now in night mode
        TextureMonitor<TR3Entities> monitor = TextureMonitor.CreateMonitor(level.Name);
        monitor.UseNightTextures = true;
    }

    private void DarkenRooms(TR3Level level)
    {
        foreach (TR3Room room in level.Rooms)
        {
            room.SetVertexLight((short)(Settings.NightModeDarkness * 10));
        }
    }
}
