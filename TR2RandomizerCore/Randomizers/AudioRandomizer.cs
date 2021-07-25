using System;
using System.Collections.Generic;
using TR2RandomizerCore.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRGE.Core;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TR2RandomizerCore.Randomizers
{
    public class AudioRandomizer : RandomizerBase
    {
        public bool ChangeTriggerTracks { get; set; }

        public TR23ScriptEditor ScriptEditor { get; set; }

        private IReadOnlyDictionary<TRAudioCategory, List<TRAudioTrack>> _tracks;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);
            _tracks = ScriptEditor.AudioProvider.GetCategorisedTracks();

            foreach (TR23ScriptedLevel lvl in Levels)
            {
                LoadLevelInstance(lvl);

                RandomizeMusicTriggers(_levelInstance);

                SaveLevelInstance();

                if (!TriggerProgress())
                {
                    break;
                }
            }
        }

        private void RandomizeMusicTriggers(TR2CombinedLevel level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level.Data);

            if (ChangeTriggerTracks)
            {
                RandomizeFloorTracks(level.Data, floorData);
            }
            RandomizeSecretTracks(level.Data, floorData);
            
            floorData.WriteToLevel(level.Data);
        }

        private void RandomizeFloorTracks(TR2Level level, FDControl floorData)
        {
            // Try to keep triggers that are beside each other and setup for the same
            // thing using the same track, otherwise the result is just a bit too random.
            // This relies on tr2audio.json having PrimaryCategory properly defined for
            // each track.
            foreach (TR2Room room in level.Rooms)
            {
                Dictionary<TRAudioCategory, TRAudioTrack> roomTracks = new Dictionary<TRAudioCategory, TRAudioTrack>();

                List<FDActionListItem> triggerItems = new List<FDActionListItem>();
                foreach (TRRoomSector sector in room.SectorList)
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
                        roomTracks[category] = tracks[_generator.Next(0, tracks.Count)];
                    }

                    item.Parameter = roomTracks[category].ID;
                }
            }
        }

        private TRAudioCategory FindTrackCategory(ushort trackID)
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

        private void RandomizeSecretTracks(TR2Level level, FDControl floorData)
        {
            // Generate new triggers for secrets to allow different sounds for each one
            List<TRAudioTrack> secretTracks = _tracks[TRAudioCategory.Secret];
            Dictionary<int, TR2Entity> secrets = GetSecretItems(level);
            foreach (int entityIndex in secrets.Keys)
            {
                TR2Entity secret = secrets[entityIndex];
                TRRoomSector sector = FDUtilities.GetRoomSector(secret.X, secret.Y, secret.Z, secret.Room, level, floorData);
                if (sector.FDIndex == 0)
                {
                    // The secret is positioned on a tile that currently has no FD, so create it
                    floorData.CreateFloorData(sector);
                }

                List<FDEntry> entries = floorData.Entries[sector.FDIndex];
                FDTriggerEntry existingTriggerEntry = entries.Find(e => e is FDTriggerEntry) as FDTriggerEntry;
                bool existingEntityPickup = false;
                if (existingTriggerEntry != null)
                {
                    if (existingTriggerEntry.TrigType == FDTrigType.Pickup && existingTriggerEntry.TrigActionList[0].Parameter == entityIndex)
                    {
                        // GW gold secret (default location) already has a pickup trigger to spawn the
                        // TRex (or whatever enemy) so we'll just append to that item list here
                        existingEntityPickup = true;
                    }
                    else
                    {
                        // There is already a non-pickup trigger for this sector so while nothing is wrong with
                        // adding a pickup trigger, the game ignores it. So in this instance, the sound that
                        // plays will just be whatever is set in the script.
                        continue;
                    }
                }

                // Generate a new music action
                FDActionListItem musicAction = new FDActionListItem
                {
                    TrigAction = FDTrigAction.PlaySoundtrack,
                    Parameter = secretTracks[_generator.Next(0, secretTracks.Count)].ID
                };

                // For GW default gold, just append it
                if (existingEntityPickup)
                {
                    existingTriggerEntry.TrigActionList.Add(musicAction);
                }
                else
                {
                    entries.Add(new FDTriggerEntry
                    {
                        // The values here are replicated from Trigger#112 (in trview) in GW.
                        // The first action list must be the entity being picked up and so
                        // remaining action list items are actioned on pick up.
                        Setup = new FDSetup { Value = 1028 },
                        TrigSetup = new FDTrigSetup { Value = 15872 },
                        TrigActionList = new List<FDActionListItem>
                        {
                            new FDActionListItem
                            {
                                TrigAction = FDTrigAction.Object,
                                Parameter = (ushort)entityIndex
                            },
                            musicAction
                        }
                    });
                }
            }
        }

        private Dictionary<int, TR2Entity> GetSecretItems(TR2Level level)
        {
            Dictionary<int, TR2Entity> entities = new Dictionary<int, TR2Entity>();
            for (int i = 0; i < level.NumEntities; i++)
            {
                TR2Entities entityType = (TR2Entities)level.Entities[i].TypeID;
                if (entityType == TR2Entities.StoneSecret_S_P || entityType == TR2Entities.JadeSecret_S_P || entityType == TR2Entities.GoldSecret_S_P)
                {
                    entities[i] = level.Entities[i];
                }
            }

            return entities;
        }
    }
}