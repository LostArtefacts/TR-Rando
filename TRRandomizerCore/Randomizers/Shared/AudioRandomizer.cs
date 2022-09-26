using System;
using System.Collections.Generic;
using System.Numerics;
using TRFDControl;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Model;
using TRRandomizerCore.Editors;
using TRRandomizerCore.SFX;

namespace TRRandomizerCore.Randomizers
{
    public class AudioRandomizer
    {
        public static readonly int FullSectorSize = 1024;
        public static readonly int HalfSectorSize = 512;

        private readonly IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> _tracks;
        private readonly Dictionary<Vector2, ushort> _trackMap;

        public AudioRandomizer(IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> tracks)
        {
            _tracks = tracks;
            _trackMap = new Dictionary<Vector2, ushort>();
        }

        public void ResetFloorMap()
        {
            _trackMap.Clear();
        }

        public void RandomizeFloorTracks(TRRoomSector[] sectors, FDControl floorData, Random generator, Func<int, Vector2> positionAction)
        {
            for (int i = 0; i < sectors.Length; i++)
            {
                TRRoomSector sector = sectors[i];
                FDActionListItem trackItem = null;
                if (sector.FDIndex > 0)
                {
                    List<FDActionListItem> actions = FDUtilities.GetActionListItems(floorData, FDTrigAction.PlaySoundtrack, sector.FDIndex);
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
                Vector2 position = positionAction.Invoke(i);
                int x = (int)position.X;
                int z = (int)position.Y;

                if (!_trackMap.ContainsKey(position))
                {
                    TRAudioCategory category = FindTrackCategory(trackItem.Parameter);
                    List<TRAudioTrack> tracks = _tracks[category];
                    _trackMap[position] = tracks[generator.Next(0, tracks.Count)].ID;
                }

                for (int xNorm = -1; xNorm < 2; xNorm++)
                {
                    for (int zNorm = -1; zNorm < 2; zNorm++)
                    {
                        int x2 = x + xNorm * FullSectorSize;
                        int z2 = z + zNorm * FullSectorSize;
                        Vector2 p2 = new Vector2(x2, z2);
                        if (!_trackMap.ContainsKey(p2))
                        {
                            _trackMap[p2] = _trackMap[position];
                        }
                    }
                }

                trackItem.Parameter = _trackMap[position];
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

        public List<TRSFXGeneralCategory> GetSFXCategories(RandomizerSettings settings)
        {
            List<TRSFXGeneralCategory> sfxCategories = new List<TRSFXGeneralCategory>();
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
}