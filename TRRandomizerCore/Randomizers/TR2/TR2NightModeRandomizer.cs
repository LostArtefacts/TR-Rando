﻿using TRGE.Core;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers;

public class TR2NightModeRandomizer : BaseTR2Randomizer
{
    public const uint DarknessRange = 10; // 0 = Dusk, 10 = Night

    internal TR2TextureMonitorBroker TextureMonitor { get; set; }

    private List<TR2ScriptedLevel> _nightLevels;

    public override void Randomize(int seed)
    {
        _generator = new Random(seed);

        Settings.NightModeDarkness = Math.Min(Settings.NightModeDarkness, DarknessRange);

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
        double scale = (100 - DarknessRange + Settings.NightModeDarkness) / 100d;

        short intensity1 = (short)(TR2Room.DarknessIntensity1 * scale);
        ushort intensity2 = (ushort)(TR2Room.DarknessIntensity2 * (2 - scale));

        foreach (TR2Room room in level.Rooms)
        {
            room.SetAmbient(intensity1);
            room.SetLights(intensity2);
            room.SetStaticMeshLights((ushort)intensity1);
            room.SetVertexLight(intensity1);
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
            List<TRStaticMesh> staticMeshes = level.StaticMeshes.ToList();
            foreach (uint meshID in _staticMeshesToHide[levelName])
            {
                TRStaticMesh mesh = staticMeshes.Find(m => m.ID == meshID);
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

    private static readonly Dictionary<string, uint[]> _staticMeshesToHide = new()
    {
        // The washing lines come in at night
        [TR2LevelNames.VENICE] = 
            new uint[] { 32, 33 },
        // The monks are washing their prayer flags
        [TR2LevelNames.MONASTERY] = 
            new uint[] { 36 }
    };
}
