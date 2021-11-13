using System;
using System.Collections.Generic;
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
        private readonly IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> _tracks;

        public AudioRandomizer(IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> tracks)
        {
            _tracks = tracks;
        }

        public void RandomizeFloorTracks(IEnumerable<TRRoomSector> sectorList, FDControl floorData, Random generator)
        {
            // Try to keep triggers that are beside each other and setup for the same thing using the same track,
            // otherwise the result is just a bit too random. This relies on the tracks having PrimaryCategory
            // properly defined.
            Dictionary<TRAudioCategory, TRAudioTrack> roomTracks = new Dictionary<TRAudioCategory, TRAudioTrack>();

            List<FDActionListItem> triggerItems = new List<FDActionListItem>();
            foreach (TRRoomSector sector in sectorList)
            {
                if (sector.FDIndex > 0)
                {
                    triggerItems.AddRange(FDUtilities.GetActionListItems(floorData, FDTrigAction.PlaySoundtrack, sector.FDIndex));
                }
            }

            // Generate a random track for the first in each category for this room, then others
            // in the same category will follow suit.
            foreach (FDActionListItem item in triggerItems)
            {
                TRAudioCategory category = FindTrackCategory(item.Parameter);
                if (!roomTracks.ContainsKey(category))
                {
                    List<TRAudioTrack> tracks = _tracks[category];
                    roomTracks[category] = tracks[generator.Next(0, tracks.Count)];
                }

                item.Parameter = roomTracks[category].ID;
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

            return sfxCategories;
        }
    }
}