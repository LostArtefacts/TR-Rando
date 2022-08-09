using System;
using System.Collections.Generic;
using System.Linq;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Conditions
{
    public class EMSecretRoomCondition : BaseEMCondition
    {
        // Does a room contain a secret?
        public short RoomIndex { get; set; }

        protected override bool Evaluate(TRLevel level)
        {
            FDControl floorData = new FDControl();
            floorData.ParseFromLevel(level);

            foreach (TRRoomSector sector in level.Rooms[RoomIndex].Sectors)
            {
                if (sector.FDIndex == 0)
                {
                    continue;
                }

                if (floorData.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) is FDTriggerEntry trigger && trigger.TrigActionList.Find(a => a.TrigAction == FDTrigAction.SecretFound) != null)
                {
                    return true;
                }
            }

            return false;
        }

        protected override bool Evaluate(TR2Level level)
        {
            List<TR2Entity> entities = level.Entities.ToList();
            return entities.Any(e => e.Room == RoomIndex && TR2EntityUtilities.IsSecretType((TR2Entities)e.TypeID));
        }

        protected override bool Evaluate(TR3Level level)
        {
            List<TR2Entity> levelEntities = level.Entities.ToList();
            List<TR2Entity> roomEntities = levelEntities.FindAll(e => e.Room == RoomIndex);
            if (roomEntities.Count > 0)
            {
                // It's difficult to tell if a particular model is being used for secret pickups,
                // so instead we check the FD under each entity in the room to see if it triggers
                // a secret found.
                FDControl floorData = new FDControl();
                floorData.ParseFromLevel(level);

                Predicate<FDEntry> pred = new Predicate<FDEntry>
                (
                    e => 
                        e is FDTriggerEntry trig && trig.TrigType == FDTrigType.Pickup
                     && trig.TrigActionList.Count > 1
                     && trig.TrigActionList[0].TrigAction == FDTrigAction.Object
                     && trig.TrigActionList[1].TrigAction == FDTrigAction.SecretFound
                );

                foreach (TR2Entity entity in roomEntities)
                {
                    TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, RoomIndex, level, floorData);
                    if (sector.FDIndex == 0)
                    {
                        continue;
                    }
                    
                    ushort entityIndex = (ushort)levelEntities.IndexOf(entity);
                    if (floorData.Entries[sector.FDIndex].Find(pred) is FDTriggerEntry trigger && trigger.TrigActionList[0].Parameter == entityIndex)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}