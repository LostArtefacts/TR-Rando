using System;
using System.Collections.Generic;
using TREnvironmentEditor.Helpers;
using TRFDControl;
using TRFDControl.FDEntryTypes;
using TRFDControl.Utilities;
using TRLevelReader.Model;

namespace TREnvironmentEditor.Model.Types
{
    public class EMMoveTriggerFunction : BaseEMFunction
    {
        public EMLocation BaseLocation { get; set; }
        public EMLocation NewLocation { get; set; }
        public int? EntityLocation { get; set; }

        public override void ApplyToLevel(TRLevel level)
        {
            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector baseSector = FDUtilities.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level, control);

            if (NewLocation != null)
            {
                NewLocation.Room = data.ConvertRoom(NewLocation.Room);
            }
            else if (EntityLocation.HasValue)
            {
                TREntity entity = level.Entities[data.ConvertEntity(EntityLocation.Value)];
                NewLocation = new EMLocation
                {
                    X = entity.X,
                    Y = entity.Y,
                    Z = entity.Z,
                    Room = data.ConvertRoom(entity.Room)
                };
            }
            else
            {
                throw new InvalidOperationException("No means to determine new sector for moving trigger.");
            }

            TRRoomSector newSector = FDUtilities.GetRoomSector(NewLocation.X, NewLocation.Y, NewLocation.Z, NewLocation.Room, level, control);
            if (MoveTriggers(baseSector, newSector, control))
            {
                control.WriteToLevel(level);

                // Make sure to copy the trigger into the flipped room if applicable
                short altRoom = level.Rooms[NewLocation.Room].AlternateRoom;
                if (altRoom != -1)
                {
                    CreateFlipmapTriggerFunction(altRoom).ApplyToLevel(level);
                }
            }
        }

        public override void ApplyToLevel(TR2Level level)
        {
            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector baseSector = FDUtilities.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level, control);

            if (NewLocation != null)
            {
                NewLocation.Room = data.ConvertRoom(NewLocation.Room);
            }
            else if (EntityLocation.HasValue)
            {
                TR2Entity entity = level.Entities[data.ConvertEntity(EntityLocation.Value)];
                NewLocation = new EMLocation
                {
                    X = entity.X,
                    Y = entity.Y,
                    Z = entity.Z,
                    Room = data.ConvertRoom(entity.Room)
                };
            }
            else
            {
                throw new InvalidOperationException("No means to determine new sector for moving trigger.");
            }

            TRRoomSector newSector = FDUtilities.GetRoomSector(NewLocation.X, NewLocation.Y, NewLocation.Z, NewLocation.Room, level, control);
            if (MoveTriggers(baseSector, newSector, control))
            {
                control.WriteToLevel(level);

                // Make sure to copy the trigger into the flipped room if applicable
                short altRoom = level.Rooms[NewLocation.Room].AlternateRoom;
                if (altRoom != -1)
                {
                    CreateFlipmapTriggerFunction(altRoom).ApplyToLevel(level);
                }
            }
        }

        public override void ApplyToLevel(TR3Level level)
        {
            EMLevelData data = GetData(level);

            FDControl control = new FDControl();
            control.ParseFromLevel(level);

            TRRoomSector baseSector = FDUtilities.GetRoomSector(BaseLocation.X, BaseLocation.Y, BaseLocation.Z, data.ConvertRoom(BaseLocation.Room), level, control);

            if (NewLocation != null)
            {
                NewLocation.Room = data.ConvertRoom(NewLocation.Room);
            }
            else if (EntityLocation.HasValue)
            {
                TR2Entity entity = level.Entities[data.ConvertEntity(EntityLocation.Value)];
                NewLocation = new EMLocation
                {
                    X = entity.X,
                    Y = entity.Y,
                    Z = entity.Z,
                    Room = data.ConvertRoom(entity.Room)
                };
            }
            else
            {
                throw new InvalidOperationException("No means to determine new sector for moving trigger.");
            }

            TRRoomSector newSector = FDUtilities.GetRoomSector(NewLocation.X, NewLocation.Y, NewLocation.Z, NewLocation.Room, level, control);
            if (MoveTriggers(baseSector, newSector, control))
            {
                control.WriteToLevel(level);

                // Make sure to copy the trigger into the flipped room if applicable
                short altRoom = level.Rooms[NewLocation.Room].AlternateRoom;
                if (altRoom != -1)
                {
                    CreateFlipmapTriggerFunction(altRoom).ApplyToLevel(level);
                }
            }
        }

        private bool MoveTriggers(TRRoomSector baseSector, TRRoomSector newSector, FDControl control)
        {
            if (baseSector != newSector && baseSector.FDIndex != 0)
            {
                List<FDEntry> triggers = control.Entries[baseSector.FDIndex].FindAll(e => e is FDTriggerEntry);
                if (triggers.Count > 0)
                {
                    if (newSector.FDIndex == 0)
                    {
                        control.CreateFloorData(newSector);
                    }

                    control.Entries[newSector.FDIndex].AddRange(triggers);

                    control.Entries[baseSector.FDIndex].RemoveAll(e => triggers.Contains(e));
                    if (control.Entries[baseSector.FDIndex].Count == 0)
                    {
                        control.RemoveFloorData(baseSector);
                    }

                    return true;
                }
            }

            return false;
        }

        private EMDuplicateTriggerFunction CreateFlipmapTriggerFunction(short altRoom)
        {
            // We want the trigger that's in the new target location added to the same
            // position in the flipped room.
            return new EMDuplicateTriggerFunction
            {
                BaseLocation = NewLocation,
                Locations = new List<EMLocation>
                {
                    new EMLocation
                    {
                        X = NewLocation.X,
                        Y = NewLocation.Y,
                        Z = NewLocation.Z,
                        Room = altRoom
                    }
                }
            };
        }
    }
}