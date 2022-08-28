using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMSwapSlotFunction : EMMoveSlotFunction
    {
        public int Slot1Index { get; set; }
        public int Slot2Index { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            if (Slot1Index == Slot2Index)
            {
                return;
            }

            TREntity slot1 = level.Entities[Slot1Index];
            TREntity slot2 = level.Entities[Slot2Index];

            // We can only swap if the slots are corresponding done/not done types.
            // So for now, just check that one doesn't have any triggers
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector slot1Sector = FDUtilities.GetRoomSector(slot1.X, slot1.Y, slot1.Z, slot1.Room, level, control);
            TRRoomSector slot2Sector = FDUtilities.GetRoomSector(slot2.X, slot2.Y, slot2.Z, slot2.Room, level, control);

            bool slot1HasTriggers = SectorHasTriggers(slot1Sector, control);
            bool slot2HasTriggers = SectorHasTriggers(slot2Sector, control);

            if (slot1HasTriggers ^ slot2HasTriggers)
            {
                if (slot1HasTriggers)
                {
                    EntityIndex = Slot1Index;
                    MoveTriggers(control, slot1Sector, slot2Sector);
                }
                else
                {
                    EntityIndex = Slot2Index;
                    MoveTriggers(control, slot2Sector, slot1Sector);
                }

                SwapSlots(slot1, slot2, GetData(level));
                control.WriteToLevel(level);
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

            // We can only swap if the slots are corresponding done/not done types.
            // So for now, just check that one doesn't have any triggers
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector slot1Sector = FDUtilities.GetRoomSector(slot1.X, slot1.Y, slot1.Z, slot1.Room, level, control);
            TRRoomSector slot2Sector = FDUtilities.GetRoomSector(slot2.X, slot2.Y, slot2.Z, slot2.Room, level, control);

            bool slot1HasTriggers = SectorHasTriggers(slot1Sector, control);
            bool slot2HasTriggers = SectorHasTriggers(slot2Sector, control);

            if (slot1HasTriggers ^ slot2HasTriggers)
            {
                if (slot1HasTriggers)
                {
                    EntityIndex = Slot1Index;
                    MoveTriggers(control, slot1Sector, slot2Sector);
                }
                else
                {
                    EntityIndex = Slot2Index;
                    MoveTriggers(control, slot2Sector, slot1Sector);
                }

                SwapSlots(slot1, slot2, GetData(level));
                control.WriteToLevel(level);
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            if (Slot1Index == Slot2Index)
            {
                return;
            }

            TR2Entity slot1 = level.Entities[Slot1Index];
            TR2Entity slot2 = level.Entities[Slot2Index];

            // We can only swap if the slots are corresponding done/not done types.
            // So for now, just check that one doesn't have any triggers
            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector slot1Sector = FDUtilities.GetRoomSector(slot1.X, slot1.Y, slot1.Z, slot1.Room, level, control);
            TRRoomSector slot2Sector = FDUtilities.GetRoomSector(slot2.X, slot2.Y, slot2.Z, slot2.Room, level, control);

            bool slot1HasTriggers = SectorHasTriggers(slot1Sector, control);
            bool slot2HasTriggers = SectorHasTriggers(slot2Sector, control);

            if (slot1HasTriggers ^ slot2HasTriggers)
            {
                if (slot1HasTriggers)
                {
                    EntityIndex = Slot1Index;
                    MoveTriggers(control, slot1Sector, slot2Sector);
                }
                else
                {
                    EntityIndex = Slot2Index;
                    MoveTriggers(control, slot2Sector, slot1Sector);
                }

                SwapSlots(slot1, slot2, GetData(level));
                control.WriteToLevel(level);
            }
        }

        private bool SectorHasTriggers(TRRoomSector sector, FDControl control)
        {
            if (sector.FDIndex != 0)
            {
                return control.Entries[sector.FDIndex].Find(e => e is FDTriggerEntry) != null;
            }

            return false;
        }

        private void SwapSlots(TREntity slot1, TREntity slot2, EMLevelData data)
        {
            EMLocation temp = new EMLocation
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

        private void SwapSlots(TR2Entity slot1, TR2Entity slot2, EMLevelData data)
        {
            EMLocation temp = new EMLocation
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
}