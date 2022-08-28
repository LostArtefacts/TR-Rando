using System;
using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public abstract class BaseMoveTriggerableFunction : BaseEMFunction
    {
        public int EntityIndex { get; set; }
        public EMLocation Location { get; set; }
        public List<EMLocation> TriggerLocations { get; set; }

        protected void RepositionTriggerable(TREntity entity, TRLevel level)
        {
            EMLevelData data = GetData(level);

            entity.X = Location.X;
            entity.Y = Location.Y;
            entity.Z = Location.Z;
            entity.Room = data.ConvertRoom(Location.Room);

            if (TriggerLocations == null || TriggerLocations.Count == 0)
            {
                // We want to keep the original triggers
                return;
            }

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            // Make a new Trigger based on the first one we find (to ensure things like one-shot are copied)
            // then copy only the action list items for this entity. But if there is already another trigger
            // on the tile, just manually copy over one-shot when appending the new action item.

            List<FDTriggerEntry> currentTriggers = FDUtilities.GetEntityTriggers(control, EntityIndex);
            FDUtilities.RemoveEntityTriggers(level, EntityIndex, control);

            AmendTriggers(currentTriggers, control, delegate (EMLocation location)
            {
                return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
            });

            control.WriteToLevel(level);
        }

        protected void RepositionTriggerable(TR2Entity entity, TR2Level level)
        {
            EMLevelData data = GetData(level);

            entity.X = Location.X;
            entity.Y = Location.Y;
            entity.Z = Location.Z;
            entity.Room = data.ConvertRoom(Location.Room);

            if (TriggerLocations == null || TriggerLocations.Count == 0)
            {
                // We want to keep the original triggers
                return;
            }

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            // Make a new Trigger based on the first one we find (to ensure things like one-shot are copied)
            // then copy only the action list items for this entity. But if there is already another trigger
            // on the tile, just manually copy over one-shot when appending the new action item.

            List<FDTriggerEntry> currentTriggers = FDUtilities.GetEntityTriggers(control, EntityIndex);
            FDUtilities.RemoveEntityTriggers(level, EntityIndex, control);

            AmendTriggers(currentTriggers, control, delegate (EMLocation location)
            {
                return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
            });

            control.WriteToLevel(level);
        }

        protected void RepositionTriggerable(TR2Entity entity, TR3Level level)
        {
            EMLevelData data = GetData(level);

            entity.X = Location.X;
            entity.Y = Location.Y;
            entity.Z = Location.Z;
            entity.Room = data.ConvertRoom(Location.Room);

            if (TriggerLocations == null || TriggerLocations.Count == 0)
            {
                return;
            }

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            List<FDTriggerEntry> currentTriggers = FDUtilities.GetEntityTriggers(control, EntityIndex);
            FDUtilities.RemoveEntityTriggers(level, EntityIndex, control);

            AmendTriggers(currentTriggers, control, delegate (EMLocation location)
            {
                return FDUtilities.GetRoomSector(location.X, location.Y, location.Z, data.ConvertRoom(location.Room), level, control);
            });

            control.WriteToLevel(level);
        }

        private void AmendTriggers(List<FDTriggerEntry> currentTriggers, FDControl control, Func<EMLocation, TRRoomSector> sectorGetter)
        {
            foreach (EMLocation location in TriggerLocations)
            {
                TRRoomSector sector = sectorGetter.Invoke(location);
                // If there is no floor data create the FD to begin with.
                if (sector.FDIndex == 0)
                {
                    control.CreateFloorData(sector);
                }

                FDActionListItem currentObjectAction = null;
                FDTriggerEntry currentTrigger = control.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) as FDTriggerEntry;
                if (currentTrigger != null)
                {
                    currentObjectAction = currentTrigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.Object);
                }

                FDActionListItem newAction = new FDActionListItem
                {
                    TrigAction = FDTrigAction.Object,
                    Parameter = (ushort)EntityIndex
                };

                if (currentObjectAction != null)
                {
                    currentTrigger.TrigActionList.Add(newAction);
                    if (currentTriggers[0].TrigSetup.OneShot)
                    {
                        currentTrigger.TrigSetup.OneShot = true;
                    }
                }
                else
                {
                    control.Entries[sector.FDIndex].Add(new FDTriggerEntry
                    {
                        Setup = currentTriggers[0].Setup,
                        TrigSetup = currentTriggers[0].TrigSetup,
                        TrigActionList = new List<FDActionListItem> { newAction }
                    });
                }
            }
        }
    }
}