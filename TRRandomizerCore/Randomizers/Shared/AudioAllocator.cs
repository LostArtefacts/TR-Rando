using System.Numerics;
using TRGE.Core;
using TRLevelControl;
using TRLevelControl.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers;

public abstract class AudioAllocator
{
    private readonly IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> _tracks;
    private readonly Dictionary<Vector2, ushort> _trackMap;
    private List<string> _uncontrolledLevels;

    public Random Generator { get; set; }
    public required RandomizerSettings Settings { get; set; }
    public List<TRSFXGeneralCategory> Categories { get; private set; }

    public AudioAllocator(IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> tracks)
    {
        _tracks = tracks;
        _trackMap = new();
    }

    public void Initialise(IEnumerable<string> levelNames, string backupPath)
    {
        Categories = GetSFXCategories();
        ChooseUncontrolledLevels(new(levelNames), GetAssaultName());
        LoadData(backupPath);
    }

    protected abstract string GetAssaultName();
    protected abstract void LoadData(string backupPath);

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
        => _uncontrolledLevels.Any(l => string.Equals(level, l, StringComparison.InvariantCultureIgnoreCase));

    public void RandomizeFloorTracks<R>(List<R> rooms, FDControl floorData)
        where R : TRRoom
    {
        if (!Settings.ChangeTriggerTracks)
        {
            return;
        }

        // TRRoomFlag.Unused2 is used in mods elsewhere to indicate that music tracks are locked.
        _trackMap.Clear();
        foreach (TRRoom room in rooms.Where(r => !r.Flags.HasFlag(TRRoomFlag.Unused2)))
        {
            RandomizeFloorTracks(room, floorData);
        }
    }

    protected void RandomizeFloorTracks(TRRoom room, FDControl floorData)
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

    public void RandomizeSecretTracks(FDControl floorData, ushort defaultTrackID)
    {
        int numSecrets = floorData.GetActionItems(FDTrigAction.SecretFound)
            .Select(a => a.Parameter)
            .Distinct().Count();
        if (numSecrets == 0 || !Settings.SeparateSecretTracks)
        {
            return;
        }

        List<TRAudioTrack> secretTracks = GetTracks(TRAudioCategory.Secret);

        for (int i = 0; i < numSecrets; i++)
        {
            TRAudioTrack secretTrack = secretTracks[Generator.Next(0, secretTracks.Count)];
            if (secretTrack.ID == defaultTrackID)
            {
                continue;
            }

            FDActionItem musicAction = new()
            {
                Action = FDTrigAction.PlaySoundtrack,
                Parameter = (short)secretTrack.ID
            };

            List<FDTriggerEntry> triggers = floorData.GetSecretTriggers(i);
            foreach (FDTriggerEntry trigger in triggers.Where(t => t.Mask == TRConsts.FullMask))
            {
                if (!trigger.Actions.Any(a => a.Action == FDTrigAction.PlaySoundtrack))
                {
                    trigger.Actions.Add(musicAction);
                }
            }
        }
    }

    public void RandomizePitch<T>(IEnumerable<TRSoundEffect<T>> effects)
        where T : Enum
    {
        if (!Settings.RandomizeWibble)
        {
            return;
        }

        // The engine does the actual randomization, we just tell it that every
        // sound effect should be included.
        foreach (TRSoundEffect<T> effect in effects)
        {
            effect.RandomizePitch = true;
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

    private List<TRSFXGeneralCategory> GetSFXCategories()
    {
        List<TRSFXGeneralCategory> sfxCategories = new();
        if (Settings.ChangeWeaponSFX)
        {
            // Pistols, Autos etc
            sfxCategories.Add(TRSFXGeneralCategory.StandardWeaponFiring);
            // Uzi/M16 - these require very short SFX so are separated
            sfxCategories.Add(TRSFXGeneralCategory.FastWeaponFiring);
            // Ricochet
            sfxCategories.Add(TRSFXGeneralCategory.Ricochet);
        }

        if (Settings.ChangeCrashSFX)
        {
            // Grenades, 40F crash, dragon explosion
            sfxCategories.Add(TRSFXGeneralCategory.Explosion);
            // Boulders settling, collapsible tiles collapsing
            sfxCategories.Add(TRSFXGeneralCategory.Clattering);
            // Gondolas, glass, ice wall
            sfxCategories.Add(TRSFXGeneralCategory.Breaking);
        }

        if (Settings.ChangeEnemySFX)
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

        if (Settings.ChangeDoorSFX)
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
