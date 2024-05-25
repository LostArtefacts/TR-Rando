using System.Numerics;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public class AudioRandomizer
{
    private readonly IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> _tracks;
    private readonly Dictionary<Vector2, ushort> _trackMap;
    private List<string> _uncontrolledLevels;

    public Random Generator { get; set; }
    public RandomizerSettings Settings { get; set; }

    public AudioRandomizer(IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> tracks)
    {
        _tracks = tracks;
        _trackMap = new();
    }

    public void ChooseUncontrolledLevels(List<string> levels, string assaultCourse)
    {
        HashSet<string> exlusions = new() { assaultCourse };
        _uncontrolledLevels = levels.RandomSelection(Generator, (int)Settings.UncontrolledSFXCount, exclusions: exlusions);
        if (Settings.UncontrolledSFXAssaultCourse)
        {
            _uncontrolledLevels.Add(assaultCourse);
        }
    }

    public bool IsUncontrolledLevel(string level)
        => _uncontrolledLevels.Contains(level);

    public void ResetFloorMap()
    {
        _trackMap.Clear();
    }

    public void RandomizeFloorTracks(TRRoom room, FDControl floorData)
    {
        for (int i = 0; i < room.Sectors.Count; i++)
        {
            TRRoomSector sector = room.Sectors[i];
            FDActionItem trackItem = null;
            if (sector.FDIndex > 0)
            {
                List<FDActionItem> actions = floorData.GetActionItems(FDTrigAction.PlaySoundtrack, sector.FDIndex);
                if (actions.Count > 0)
                {
                    trackItem = actions[0];
                }
            }

            if (trackItem == null)
            {
                continue;
            }

            // Get this sector's midpoint in world coordinates. Store each immediately
            // neighbouring tile to use the same track as this one, regardless of room.
            Vector2 position = new
            (
                TRConsts.Step2 + room.Info.X + i / room.NumZSectors * TRConsts.Step4,
                TRConsts.Step2 + room.Info.Z + i % room.NumZSectors * TRConsts.Step4
            );
            int x = (int)position.X;
            int z = (int)position.Y;

            if (!_trackMap.ContainsKey(position))
            {
                TRAudioCategory category = FindTrackCategory((ushort)trackItem.Parameter);
                List<TRAudioTrack> tracks = _tracks[category];
                _trackMap[position] = tracks[Generator.Next(0, tracks.Count)].ID;
            }

            for (int xNorm = -1; xNorm < 2; xNorm++)
            {
                for (int zNorm = -1; zNorm < 2; zNorm++)
                {
                    int x2 = x + xNorm * TRConsts.Step4;
                    int z2 = z + zNorm * TRConsts.Step4;
                    Vector2 p2 = new(x2, z2);
                    if (!_trackMap.ContainsKey(p2))
                    {
                        _trackMap[p2] = _trackMap[position];
                    }
                }
            }

            trackItem.Parameter = (short)_trackMap[position];
        }
    }

    public TRAudioCategory FindTrackCategory(ushort trackID)
    {
        foreach (TRAudioCategory category in _tracks.Keys)
        {
            foreach (TRAudioTrack track in _tracks[category])
            {
                if (track.ID == trackID)
                {
                    return track.PrimaryCategory;
                }
            }
        }

        return TRAudioCategory.General;
    }

    public List<TRAudioTrack> GetTracks(TRAudioCategory category)
    {
        return _tracks[category];
    }

    public static List<TRSFXGeneralCategory> GetSFXCategories(RandomizerSettings settings)
    {
        List<TRSFXGeneralCategory> sfxCategories = new();
        if (settings.ChangeWeaponSFX)
        {
            // Pistols, Autos etc
            sfxCategories.Add(TRSFXGeneralCategory.StandardWeaponFiring);
            // Uzi/M16 - these require very short SFX so are separated
            sfxCategories.Add(TRSFXGeneralCategory.FastWeaponFiring);
            // Ricochet
            sfxCategories.Add(TRSFXGeneralCategory.Ricochet);
        }

        if (settings.ChangeCrashSFX)
        {
            // Grenades, 40F crash, dragon explosion
            sfxCategories.Add(TRSFXGeneralCategory.Explosion);
            // Boulders settling, collapsible tiles collapsing
            sfxCategories.Add(TRSFXGeneralCategory.Clattering);
            // Gondolas, glass, ice wall
            sfxCategories.Add(TRSFXGeneralCategory.Breaking);
        }

        if (settings.ChangeEnemySFX)
        {
            // General death noises
            sfxCategories.Add(TRSFXGeneralCategory.Death);
            // Enemies getting hit
            sfxCategories.Add(TRSFXGeneralCategory.TakingDamage);
            // Standard footsteps, shuffles/scrapes (like Flamethrower & Winston)
            sfxCategories.Add(TRSFXGeneralCategory.StandardFootstep);
            // Chicken, T-Rex, Dragon
            sfxCategories.Add(TRSFXGeneralCategory.HeavyFootstep);
            // E.g. ShotgunGoon laughing, Gunman1/2 breathing, Doberman panting
            sfxCategories.Add(TRSFXGeneralCategory.Breathing);
            // Loosely categorised as "bored" enemies, like the yetis wandering before Lara approaches
            sfxCategories.Add(TRSFXGeneralCategory.Grunting);
            // Enemies in attack mode, like tigers growling at Lara
            sfxCategories.Add(TRSFXGeneralCategory.Growling);
            // Enemies alerted by Lara
            sfxCategories.Add(TRSFXGeneralCategory.Alerting);
            // Wing flaps, Tinnos wasps
            sfxCategories.Add(TRSFXGeneralCategory.Flying);
        }

        if (settings.ChangeDoorSFX)
        {
            // Doors that share opening/closing sounds
            sfxCategories.Add(TRSFXGeneralCategory.GeneralDoor);
            // Opening doors/trapdoors
            sfxCategories.Add(TRSFXGeneralCategory.DoorOpening);
            // Closing trapdoors
            sfxCategories.Add(TRSFXGeneralCategory.DoorClosing);
            // Switches/levelrs that share opening/closing sounds
            sfxCategories.Add(TRSFXGeneralCategory.GeneralSwitch);
            // Pulling switch up
            sfxCategories.Add(TRSFXGeneralCategory.SwitchUp);
            // Pulling switch down
            sfxCategories.Add(TRSFXGeneralCategory.SwitchDown);
        }

        return sfxCategories;
    }
}
