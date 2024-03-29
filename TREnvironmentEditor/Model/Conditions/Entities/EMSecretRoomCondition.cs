﻿using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelControl.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Conditions;

public class EMSecretRoomCondition : BaseEMCondition
{
    // Does a room contain a secret?
    public short RoomIndex { get; set; }

    protected override bool Evaluate(TR1Level level)
    {
        FDControl floorData = new();
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
        return level.Entities.Any(e => e.Room == RoomIndex && TR2TypeUtilities.IsSecretType(e.TypeID));
    }

    protected override bool Evaluate(TR3Level level)
    {
        List<TR3Entity> roomEntities = level.Entities.FindAll(e => e.Room == RoomIndex);
        if (roomEntities.Count > 0)
        {
            // It's difficult to tell if a particular model is being used for secret pickups,
            // so instead we check the FD under each entity in the room to see if it triggers
            // a secret found.
            FDControl floorData = new();
            floorData.ParseFromLevel(level);

            Predicate<FDEntry> pred = new(
                e => 
                    e is FDTriggerEntry trig && trig.TrigType == FDTrigType.Pickup
                 && trig.TrigActionList.Count > 1
                 && trig.TrigActionList[0].TrigAction == FDTrigAction.Object
                 && trig.TrigActionList[1].TrigAction == FDTrigAction.SecretFound
            );

            foreach (TR3Entity entity in roomEntities)
            {
                TRRoomSector sector = FDUtilities.GetRoomSector(entity.X, entity.Y, entity.Z, RoomIndex, level, floorData);
                if (sector.FDIndex == 0)
                {
                    continue;
                }
                
                ushort entityIndex = (ushort)level.Entities.IndexOf(entity);
                if (floorData.Entries[sector.FDIndex].Find(pred) is FDTriggerEntry trigger && trigger.TrigActionList[0].Parameter == entityIndex)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
