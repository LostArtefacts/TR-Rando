using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TR2NightModeRandomizer : BaseTR2Randomizer
{
    internal TR2TextureMonitorBroker TextureMonitor { get; set; }

    private List<TR2ScriptedLevel> _nightLevels;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        Settings.NightModeDarkness = Math.Min(Settings.NightModeDarkness, RandoConsts.DarknessRange);

        ChooseNightLevels();

        foreach (TR2ScriptedLevel lvl in Levels)
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
        TR2ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TR2LevelNames.ASSAULT));
        ISet<TR2ScriptedLevel> exlusions = new HashSet<TR2ScriptedLevel> { assaultCourse };

        _nightLevels = Levels.RandomSelection(_generator, (int)Settings.NightModeCount, exclusions: exlusions);
        if (Settings.NightModeAssaultCourse)
        {
            _nightLevels.Add(assaultCourse);
        }
    }

    private void SetNightMode(TR2CombinedLevel level)
    {
        DarkenRooms(level.Data);
        HideDaytimeEntities(level.Data, level.Name);

        if (Settings.OverrideSunsets)
        {
            level.Script.HasSunset = false;
        }

        if (level.HasCutScene)
        {
            SetNightMode(level.CutSceneLevel);
        }

        // Notify the texture monitor that this level is now in night mode
        TextureMonitor<TR2Type> monitor = TextureMonitor.CreateMonitor(level.Name);
        monitor.UseNightTextures = true;
    }

    private void DarkenRooms(TR2Level level)
    {
        double scale = (100 - RandoConsts.DarknessRange + Settings.NightModeDarkness) / 100d;

        short intensity1 = (short)(RandoConsts.DarknessIntensity1 * scale);
        ushort intensity2 = (ushort)(RandoConsts.DarknessIntensity2 * (2 - scale));

        foreach (TR2Room room in level.Rooms)
        {
            SetAmbient(room, intensity1);
            SetLights(room, intensity2);
            SetStaticMeshLights(room, (ushort)intensity1);
            SetVertexLight(room, intensity1);
        }
    }

    private static void SetAmbient(TR2Room room, short val)
    {
        room.AmbientIntensity = val;
        room.AmbientIntensity2 = val;
    }

    private static void SetLights(TR2Room room, ushort val)
    {
        foreach (TR2RoomLight light in room.Lights)
        {
            light.Intensity1 = val;
            light.Intensity2 = val;
        }
    }

    private static void SetStaticMeshLights(TR2Room room, ushort val)
    {
        foreach (TR2RoomStaticMesh mesh in room.StaticMeshes)
        {
            mesh.Intensity1 = val;
            mesh.Intensity2 = val;
        }
    }

    private static void SetVertexLight(TR2Room room, short val)
    {
        foreach (TR2RoomVertex vert in room.Mesh.Vertices)
        {
            vert.Lighting = val;
            vert.Lighting2 = val;
        }
    }

    private void HideDaytimeEntities(TR2Level level, string levelName)
    {
        // A list of item locations to choose from
        List<TR2Entity> items = level.Entities.Where
        (
            e =>
                TR2TypeUtilities.IsAmmoType(e.TypeID) ||
                TR2TypeUtilities.IsGunType(e.TypeID) ||
                TR2TypeUtilities.IsUtilityType(e.TypeID)
        ).ToList();

        foreach (TR2Type entityToReplace in _entitiesToReplace.Keys)
        {
            IEnumerable<TR2Entity> ents = level.Entities.Where(e => e.TypeID == entityToReplace);
            foreach (TR2Entity entity in ents)
            {
                TR2Entity item = items[_generator.Next(0, items.Count)];
                entity.TypeID = _entitiesToReplace[entityToReplace];
                entity.Room = item.Room;
                entity.X = item.X;
                entity.Y = item.Y;
                entity.Z = item.Z;
                entity.Intensity1 = item.Intensity1;
                entity.Intensity2 = item.Intensity2;
            }
        }

        // Hide any static meshes
        if (_staticMeshesToHide.ContainsKey(levelName))
        {
            foreach (TR2Type type in _staticMeshesToHide[levelName])
            {
                TRStaticMesh mesh = level.StaticMeshes[type];
                if (mesh != null)
                {
                    mesh.NonCollidable = true;
                    mesh.Visible = false;
                }
            }
        }
    }

    private static readonly Dictionary<TR2Type, TR2Type> _entitiesToReplace = new()
    {
        [TR2Type.SingingBirds_N] = TR2Type.Flares_S_P // Birds don't sing at night
    };

    private static readonly Dictionary<string, TR2Type[]> _staticMeshesToHide = new()
    {
        // The washing lines come in at night
        [TR2LevelNames.VENICE] = 
            new TR2Type[] { TR2Type.Architecture2, TR2Type.Architecture3 },
        // The monks are washing their prayer flags
        [TR2LevelNames.MONASTERY] = 
            new TR2Type[] { TR2Type.Architecture6 }
    };
}
