using TREnvironmentEditor.Helpers;
using TRLevelControl.Model;

namespace TREnvironmentEditor.Model.Types;

public class EMSwapSlotFunction : EMMoveSlotFunction
{
    public int Slot1Index { get; set; }
    public int Slot2Index { get; set; }

    public override void ApplyToLevel(TR1Level level)
    {
        if (Slot1Index == Slot2Index)
        {
            return;
        }

        TR1Entity slot1 = level.Entities[Slot1Index];
        TR1Entity slot2 = level.Entities[Slot2Index];

        // We can only swap if the slots are corresponding done/not done types.
        // So for now, just check that one doesn't have any triggers

        TRRoomSector slot1Sector = level.GetRoomSector(slot1.X, slot1.Y, slot1.Z, slot1.Room);
        TRRoomSector slot2Sector = level.GetRoomSector(slot2.X, slot2.Y, slot2.Z, slot2.Room);

        bool slot1HasTriggers = SectorHasTriggers(slot1Sector, level.FloorData);
        bool slot2HasTriggers = SectorHasTriggers(slot2Sector, level.FloorData);

        if (slot1HasTriggers ^ slot2HasTriggers)
        {
            if (slot1HasTriggers)
            {
                EntityIndex = Slot1Index;
                MoveTriggers(level.FloorData, slot1Sector, slot2Sector);
            }
            else
            {
                EntityIndex = Slot2Index;
                MoveTriggers(level.FloorData, slot2Sector, slot1Sector);
            }

            SwapSlots(slot1, slot2, GetData(level));
        }
    }

    public override void ApplyToLevel(TR2Level level)
    {
        if (Slot1Index == Slot2Index)
        {
            return;
        }

        TR2Entity slot1 = level.Entities[Slot1Index];
        TR2Entity slot2 = level.Entities[Slot2Index];

        TRRoomSector slot1Sector = level.GetRoomSector(slot1.X, slot1.Y, slot1.Z, slot1.Room);
        TRRoomSector slot2Sector = level.GetRoomSector(slot2.X, slot2.Y, slot2.Z, slot2.Room);

        bool slot1HasTriggers = SectorHasTriggers(slot1Sector, level.FloorData);
        bool slot2HasTriggers = SectorHasTriggers(slot2Sector, level.FloorData);

        if (slot1HasTriggers ^ slot2HasTriggers)
        {
            if (slot1HasTriggers)
            {
                EntityIndex = Slot1Index;
                MoveTriggers(level.FloorData, slot1Sector, slot2Sector);
            }
            else
            {
                EntityIndex = Slot2Index;
                MoveTriggers(level.FloorData, slot2Sector, slot1Sector);
            }

            SwapSlots(slot1, slot2, GetData(level));
        }
    }

    public override void ApplyToLevel(TR3Level level)
    {
        if (Slot1Index == Slot2Index)
        {
            return;
        }

        TR3Entity slot1 = level.Entities[Slot1Index];
        TR3Entity slot2 = level.Entities[Slot2Index];

        TRRoomSector slot1Sector = level.GetRoomSector(slot1.X, slot1.Y, slot1.Z, slot1.Room);
        TRRoomSector slot2Sector = level.GetRoomSector(slot2.X, slot2.Y, slot2.Z, slot2.Room);

        bool slot1HasTriggers = SectorHasTriggers(slot1Sector, level.FloorData);
        bool slot2HasTriggers = SectorHasTriggers(slot2Sector, level.FloorData);

        if (slot1HasTriggers ^ slot2HasTriggers)
        {
            if (slot1HasTriggers)
            {
                EntityIndex = Slot1Index;
                MoveTriggers(level.FloorData, slot1Sector, slot2Sector);
            }
            else
            {
                EntityIndex = Slot2Index;
                MoveTriggers(level.FloorData, slot2Sector, slot1Sector);
            }

            SwapSlots(slot1, slot2, GetData(level));
        }
    }

    private static bool SectorHasTriggers(TRRoomSector sector, FDControl control)
    {
        return sector.FDIndex != 0
            && control[sector.FDIndex].Any(e => e is FDTriggerEntry);
    }

    private static void SwapSlots<T>(TREntity<T> slot1, TREntity<T> slot2, EMLevelData data)
        where T : Enum
    {
        EMLocation temp = new()
        {
            X = slot1.X,
            Y = slot1.Y,
            Z = slot1.Z,
            Room = slot1.Room,
            Angle = slot1.Angle
        };

        slot1.X = slot2.X;
        slot1.Y = slot2.Y;
        slot1.Z = slot2.Z;
        slot1.Room = data.ConvertRoom(slot2.Room);
        slot1.Angle = slot2.Angle;

        slot2.X = temp.X;
        slot2.Y = temp.Y;
        slot2.Z = temp.Z;
        slot2.Room = data.ConvertRoom(temp.Room);
        slot2.Angle = temp.Angle;
    }
}
