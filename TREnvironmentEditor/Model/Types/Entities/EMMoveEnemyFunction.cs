using System.Collections.Generic;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveEnemyFunction : BaseEMFunction
    {
        public int EntityIndex { get; set; }
        public bool IfLandCreature { get; set; }
        public bool AttemptWaterCreature { get; set; }
        public EMLocation Location { get; set; }
        public List<EMLocation> TriggerLocations { get; set; }

        public override void ApplyToLevel(TR2Level level)
        {
            TR2Entity enemy = level.Entities[EntityIndex];
            TR2Entities enemyEntity = (TR2Entities)enemy.TypeID;
            bool isWaterEnemy = TR2EntityUtilities.IsWaterCreature(enemyEntity);

            // If the index doesn't point to an enemy or if we only want to move land creatures
            // but the enemy is a water creature (and vice-versa), bail out.
            if (!TR2EntityUtilities.IsEnemyType(enemyEntity) || (IfLandCreature && isWaterEnemy) || (!IfLandCreature && !isWaterEnemy))
            {
                return;
            }

            // If the level has water creatures available, and we want to switch it, do so.
            if (AttemptWaterCreature)
            {
                TR2Entity waterEnemy = level.Entities.ToList().Find(e => TR2EntityUtilities.IsWaterCreature((TR2Entities)e.TypeID));
                if (waterEnemy != null)
                {
                    enemy.TypeID = waterEnemy.TypeID;
                    return;
                }
            }

            // Otherwise, reposition the enemy and its triggers.

            enemy.X = Location.X;
            enemy.Y = Location.Y;
            enemy.Z = Location.Z;
            enemy.Room = Location.Room;

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
            
            foreach (EMLocation location in TriggerLocations)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(location.X, location.Y, location.Z, location.Room, level, control);
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
                        currentTrigger.TrigSetup.SetOneShot();
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

            control.WriteToLevel(level);
        }
    }
}