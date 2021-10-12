using System;
using System.Collections.Generic;
using TRFDControl;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Model;

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
    }
}