using System;
using TREnvironmentEditor.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;

namespace TREnvironmentEditor.Model.Types
{
    public class EMConvertWheelDoorFunction : BaseEMFunction
    {
        public int WheelIndex { get; set; }
        public int DoorIndex { get; set; }
        public short NewSwitchType { get; set; }
        public short NewDoorType { get; set; }
        public EMLocation NewLocation { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            throw new NotSupportedException();
        }

        public override void ApplyToLevel(TR2Level level)
        {
            TR2Entity wheel = level.Entities[WheelIndex];
            if (wheel.TypeID != (short)TR2Entities.WheelKnob)
            {
                // Something else has already converted this
                return;
            }

            // Make it a switch type
            new EMConvertEntityFunction
            {
                EntityIndex = WheelIndex,
                NewEntityType = NewSwitchType
            }.ApplyToLevel(level);

            // Make the door normal and match its lighting to another
            TR2Entity otherDoor = Array.Find(level.Entities, e => e.TypeID == NewDoorType);
            new EMConvertEntityFunction
            {
                EntityIndex = DoorIndex,
                NewEntityType = NewDoorType
            }.ApplyToLevel(level);
            if (otherDoor != null)
            {
                level.Entities[DoorIndex].Intensity1 = otherDoor.Intensity1;
                level.Entities[DoorIndex].Intensity2 = otherDoor.Intensity2;
            }

            if (NewLocation == null)
            {
                // This indicates that we want to get rid of the wheel and make its current trigger a pad
                new EMConvertTriggerFunction
                {
                    TrigType = TRFDControl.FDTrigType.Pad,
                    Location = new EMLocation
                    {
                        X = wheel.X,
                        Y = wheel.Y,
                        Z = wheel.Z,
                        Room = wheel.Room
                    }
                }.ApplyToLevel(level);
            }
            else
            {
                // Make sure the trigger has the switch ref
                new EMConvertTriggerFunction
                {
                    SwitchOrKeyRef = (ushort)WheelIndex,
                    Location = new EMLocation
                    {
                        X = wheel.X,
                        Y = wheel.Y,
                        Z = wheel.Z,
                        Room = wheel.Room
                    }
                }.ApplyToLevel(level);

                // Finally, move the switch
                new EMMoveSlotFunction
                {
                    EntityIndex = WheelIndex,
                    Location = NewLocation
                }.ApplyToLevel(level);
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            throw new NotSupportedException();
        }
    }
}